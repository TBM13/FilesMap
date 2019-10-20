using System;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using FilesMap.Properties;

namespace FilesMap
{
    public partial class MainWindow : Window
    {
        private string[] data;
        private string defaultDrive;

        private double previousWidth = 0;

        private int tilesCount;
        private int column = 0;

        private string currentPath;
        private readonly StringCollection previousPath = new StringCollection();

        private string forceInterpret = "";

        private bool clickedOnGrid;

        private string elementToRename;

        public MainWindow(string[] _data, string _defaultDrive, string _forceInterpret)
        {
            InitializeComponent();

            data = _data;
            defaultDrive = _defaultDrive;
            forceInterpret = _forceInterpret;

            SizeChanged += Window_SizeChanged;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => Environment.Exit(0);

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double a = e.NewSize.Width;
            if (a > previousWidth + 80 || a < previousWidth - 80 || Tiles_Scroll.ComputedHorizontalScrollBarVisibility == Visibility.Visible)
            {
                previousWidth = a;
                Navigate(currentPath);
            }
        }

        private void AddTile(string _path)
        {
            if ((80 * tilesCount) >= (ActualWidth - 80))
            {
                column++;
                tilesCount = 0;
            }

            int a = 10 + (80 * tilesCount);
            int b = 90 * column;

            string name = _path.Remove(0, _path.LastIndexOf(Settings.Default.DirSeparator) + 1);
            string extension = "DIR";
            if (_path.Remove(0, _path.LastIndexOf(Settings.Default.DirSeparator)).Contains(Settings.Default.FileExtSeparator))
            {
                extension = _path.Remove(0, _path.LastIndexOf(Settings.Default.FileExtSeparator) + 1).ToUpper();
                if (extension == "DIR") extension = "NULL";
            }

            ContextMenu contextMenu = new ContextMenu();

            MenuItem deleteElement = new MenuItem
            {
                Header = "Delete"
            };

            MenuItem renameElement = new MenuItem
            {
                Header = "Rename"
            };

            Separator separator = new Separator();

            MenuItem interpretAsAFolder = new MenuItem
            {
                Header = "Interpret as a folder"
            };

            MenuItem interpretAsAFile = new MenuItem
            {
                Header = "Interpret as a file"
            };

            Grid tileBackground = new Grid
            {
                ToolTip = name,
                Width = 64,
                Height = 64,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                ContextMenu = contextMenu,
                Margin = new Thickness(a, b, 0, 0)
            };

            Image tile = new Image
            {
                Width = 48,
                Height = 48,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Source = GetFileIcon(extension, _path),
                Margin = new Thickness(8, 0, 0, 0)
            };

            TextBlock tileText = new TextBlock
            {
                Text = name,
                Width = 64,
                Height = 32,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(0, 44, 0, 0)
            };

            Typeface typeface = new Typeface(tileText.FontFamily, tileText.FontStyle, tileText.FontWeight, tileText.FontStretch);
            FormattedText formmatedText = new FormattedText(tileText.Text, System.Threading.Thread.CurrentThread.CurrentCulture, tileText.FlowDirection, typeface, tileText.FontSize, tileText.Foreground, VisualTreeHelper.GetDpi(this).PixelsPerDip);

            while (formmatedText.Width > tileText.Width && tileBackground.Height < 80) tileBackground.Height += 5;

            contextMenu.Items.Add(deleteElement);
            contextMenu.Items.Add(renameElement);
            contextMenu.Items.Add(separator);
            contextMenu.Items.Add(tile.Source.ToString() == "pack://application:,,,/Images/dir64.png" ? interpretAsAFile : interpretAsAFolder);

            deleteElement.Click += (sender2, e2) =>
            {
                if (!Settings.Default.DeleteConfirmation || MessageBox.Show("Are you sure that you want to delete this element?", "FilesMap", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        if (data[i] == _path)
                        {
                            data[i] = data[data.Length - 1];
                            Array.Resize(ref data, data.Length - 1);
                            Navigate(currentPath);
                            break;
                        }
                    }
                }
            };

            renameElement.Click += (sender2, e2) =>
            {
                elementToRename = _path;
                Btn_CreateElement_Create.Content = "Rename";
                Txt_CreateElement_Name.Text = name;
                Grid_CreateElement.Visibility = Visibility.Visible;
                Txt_CreateElement_Name.Focus();
                if (Settings.Default.EnableEffects)
                    Grid_BlurEffect.Radius = 2;
                else
                    Grid_CreateElement_Shadow.Opacity = 0;
            };

            interpretAsAFolder.Click += (sender2, e2) => ForceInterpret(_path);
            interpretAsAFile.Click += (sender2, e2) => ForceInterpret(_path);

            tileBackground.MouseUp += (sender2, e2) => TileMouseUp(sender2, e2, tile, _path);
            tileBackground.MouseEnter += (sender2, e2) => TileMouseEnter(sender2, e2, tileBackground);
            tileBackground.MouseLeave += (sender2, e2) => TileMouseLeave(sender2, e2, tileBackground);

            tileBackground.Children.Add(tile);
            tileBackground.Children.Add(tileText);
            TilesContent.Children.Add(tileBackground);
            tilesCount++;
        }

        private void ClearTiles()
        {
            TilesContent.Children.Clear();
            column = 0;
            tilesCount = 0;
        }

        public void Navigate(string _path, bool goBack = false)
        {
            try
            {
                if (goBack) previousPath.RemoveAt(previousPath.Count - 1);

                string dirSeparator = Settings.Default.DirSeparator;

                ClearTiles();
                foreach (string element in data)
                {
                    string elem = element;
                    if (elem.Substring(elem.Length - 1) == dirSeparator) elem = elem.Remove(elem.Length - 1, 1);
                    if (_path.Substring(_path.Length - 1) != dirSeparator) _path += dirSeparator;
                    if (elem.Contains(_path) && elem.Length - elem.Replace(dirSeparator, "").Length == _path.Length - _path.Replace(dirSeparator, "").Length)
                    {
                        AddTile(elem);
                    }
                }

                if (!goBack && currentPath != null && _path != currentPath)
                {
                    if (previousPath.Count > 0)
                    {
                        if (previousPath[previousPath.Count - 1] != currentPath) previousPath.Add(currentPath);
                    }
                    else
                    {
                        previousPath.Add(currentPath);
                    }
                }

                Textbox_Path.Text = _path;
                currentPath = _path;

                Btn_Back.IsEnabled = previousPath.Count > 0;
            }
            catch (Exception e)
            {
                MessageBox.Show("An error has occurred:\n" + e.ToString(), "FilesMap", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

#pragma warning disable IDE0060
        private void TileMouseUp(object sender, MouseButtonEventArgs e, Image tile, string _path)
        {
            if (e.ChangedButton == MouseButton.Left && tile.Source.ToString() == "pack://application:,,,/Images/dir64.png") Navigate(_path);
        }

        private void TileMouseEnter(object sender, MouseEventArgs e, Grid tileBackground) => tileBackground.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#e3f1ff");
        private void TileMouseLeave(object sender, MouseEventArgs e, Grid tileBackground) => tileBackground.Background = Brushes.Transparent;
#pragma warning restore IDE0060

        private void Button_Click(object sender, RoutedEventArgs e) => Navigate(Textbox_Path.Text.Length > 0 ? Textbox_Path.Text : defaultDrive);

        private BitmapImage GetFileIcon(string extension, string _path = "")
        {
            if (extension == "7Z" || extension == "RAR" || extension == "TAR")
                extension = "zip";
            else if (extension == "JPEG")
                extension = "jpg";
            else if (extension == "DOCX")
                extension = "doc";
            else if (extension == "SH" || extension == "CMD")
                extension = "bat";

            if (_path.Length > 0 && forceInterpret.Contains(_path + ";"))
                extension = extension == "DIR" ? "" : "dir";

            BitmapImage elemIcon = new BitmapImage();
            elemIcon.BeginInit();
            elemIcon.UriSource = HasIcon(extension.ToUpper()) ? new Uri("pack://application:,,,/Images/" + extension.ToLower() + "64.png", UriKind.Absolute) : new Uri("pack://application:,,,/Images/file64.png", UriKind.Absolute);
            elemIcon.EndInit();
            return elemIcon;
        }

        private bool HasIcon(string extension) => extension == "DIR" || extension == "AVI" || extension == "CSS" || extension == "DLL" || extension == "DOC" || extension == "DOCX" || extension == "EXE" || extension == "HTML" || extension == "ISO" || extension == "JPG" || extension == "JPEG" || extension == "JS" || extension == "JSON" || extension == "MP3" || extension == "MP4" || extension == "PDF" || extension == "PNG" || extension == "PSD" || extension == "RTF" || extension == "SVG" || extension == "TXT" || extension == "XML" || extension == "ZIP" || extension == "7Z" || extension == "RAR" || extension == "TAR" || extension == "INI" || extension == "JAR" || extension == "BAT" || extension == "SH" || extension == "CMD" || extension == "GIF" || extension == "LNK";

        private void Btn_Back_Click(object sender, RoutedEventArgs e) => Navigate(previousPath[previousPath.Count - 1], true);

        private void ForceInterpret(string _path)
        {
            if (forceInterpret.Contains(_path + ";"))
                forceInterpret = forceInterpret.Replace(_path + ";", "");
            else
                forceInterpret += _path + ";";

            ClearTiles();
            Navigate(currentPath);
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e) => Menu_ExportInterpretData.IsEnabled = forceInterpret.Length > 0;

        private void Menu_ExportInterpretData_Click(object sender, RoutedEventArgs e)
        {
            string fileName = ShowSaveDialog("Export interpret as a file/folder data");

            if (fileName.Length > 0)
                File.WriteAllText(fileName, forceInterpret);
        }

        private void Menu_ExportData_Click(object sender, RoutedEventArgs e)
        {
            string fileName = ShowSaveDialog("Export data");

            if (fileName.Length > 0)
            {
                string a = "";
                foreach (string element in data)
                {
                    a += element + "\n";
                }

                File.WriteAllText(fileName, a);
            }
        }

        private string ShowSaveDialog(string title)
        {
            SaveFileDialog SFD = new SaveFileDialog
            {
                Title = title,
                RestoreDirectory = true,
                Filter = "Text files|*.txt|All files|*.*",
                DefaultExt = "txt"
            };

            return SFD.ShowDialog() == true ? SFD.FileName : "";
        }

        private void Menu_CreateElement_Click(object sender, RoutedEventArgs e)
        {
            Btn_CreateElement_Create.Content = "Create";
            Txt_CreateElement_Name.Clear();
            Grid_CreateElement.Visibility = Visibility.Visible;
            Txt_CreateElement_Name.Focus();
            if (Settings.Default.EnableEffects)
                Grid_BlurEffect.Radius = 2;
            else
                Grid_CreateElement_Shadow.Opacity = 0;
        }

        private void Grid_CreateElement_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!clickedOnGrid)
            {
                Grid_CreateElement.Visibility = Visibility.Hidden;
                Grid_BlurEffect.Radius = 0;
            }

            clickedOnGrid = false;
        }

        private void Grid_CreateElement2_MouseDown(object sender, MouseButtonEventArgs e) => clickedOnGrid = true;

        private void Btn_CreateElement_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Grid_CreateElement.Visibility = Visibility.Hidden;
            Grid_BlurEffect.Radius = 0;
        }

        //This is here to prevent grid from closing when right click is pressed on Txt_CreateElement_Name
        private void Txt_CreateElement_Name_MouseLeave(object sender, MouseEventArgs e) => clickedOnGrid = false;
        private void Txt_CreateElement_Name_MouseEnter(object sender, MouseEventArgs e) => clickedOnGrid = true;

        private void Btn_CreateElement_Create_Click(object sender, RoutedEventArgs e)
        {
            if (Txt_CreateElement_Name.Text.Contains(Settings.Default.DirSeparator) || Txt_CreateElement_Name.Text.Contains(Settings.Default.DriveSeparator))
            {
                MessageBox.Show($"The element name can't contain these characters:\n{Settings.Default.DirSeparator}  {Settings.Default.DriveSeparator}", "FilesMap", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string newElementPath = currentPath + Txt_CreateElement_Name.Text + Settings.Default.DirSeparator;

            foreach (string element in data)
            {
                if (element == newElementPath)
                {
                    MessageBox.Show("The element '" + Txt_CreateElement_Name.Text + "' already exists in this path.", "FilesMap", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (Btn_CreateElement_Create.Content.ToString() == "Create")
            {
                Array.Resize(ref data, data.Length + 1);
                data[data.Length - 1] = newElementPath;
                AddTile(newElementPath.Remove(newElementPath.Length - 1, 1));
            }
            else
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i].Contains(elementToRename))
                    {
                        data[i] = data[i].Replace(elementToRename, newElementPath);
                    }
                }

                Navigate(currentPath);
            }

            Grid_CreateElement.Visibility = Visibility.Hidden;
            Grid_BlurEffect.Radius = 0;
        }

        private void Txt_CreateElement_Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            Btn_CreateElement_Create.IsEnabled = Txt_CreateElement_Name.Text.Length > 0;

            string extension = "DIR";
            if (Txt_CreateElement_Name.Text.Contains(Settings.Default.FileExtSeparator))
            {
                extension = Txt_CreateElement_Name.Text.ToUpper().Remove(0, Txt_CreateElement_Name.Text.LastIndexOf(Settings.Default.FileExtSeparator) + 1);
                if (extension == "DIR") extension = "NULL";
            }

            Image_CreateElement_Icon.Source = GetFileIcon(extension); 
        }
    }
}
