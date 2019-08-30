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
        public string[] data;
        public string defaultDrive;

        private double previousWidth = 0;

        private int tilesCount;
        private int column = 0;

        private string currentPath;
        private readonly StringCollection previousPath = new StringCollection();

        public string forceInterpret = "";

        public MainWindow() => InitializeComponent();

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => Environment.Exit(0);

        private void Window_Loaded(object sender, RoutedEventArgs e) => ShowPrompt();

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double a = e.NewSize.Width;
            if (a > previousWidth + 80 || a < previousWidth - 80 || Tiles_Scroll.ComputedHorizontalScrollBarVisibility == Visibility.Visible)
            {
                previousWidth = a;
                Navigate(currentPath);
            }
        }

        private void ShowPrompt()
        {
            Visibility = Visibility.Hidden;
            Prompt prompt = new Prompt(this);
            if (prompt.ShowDialog() != true || data.Length == 0) Environment.Exit(0);
            SizeChanged += Window_SizeChanged;
            Visibility = Visibility.Visible;
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
            string extension = _path.Remove(0, _path.LastIndexOf(Settings.Default.DirSeparator)).Contains(Settings.Default.FileExtSeparator) ? _path.Remove(0, _path.LastIndexOf(Settings.Default.FileExtSeparator) + 1).ToUpper() : "DIR";

            ContextMenu contextMenu = new ContextMenu();

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

            contextMenu.Items.Add(tile.Source.ToString() == "pack://application:,,,/Images/dir64.png" ? interpretAsAFile : interpretAsAFolder);

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

        private BitmapImage GetFileIcon(string extension, string _path)
        {
            if (extension == "7Z" || extension == "RAR" || extension == "TAR")
                extension = "zip";
            else if (extension == "JPEG")
                extension = "jpg";
            else if (extension == "DOCX")
                extension = "doc";
            else if (extension == "SH" || extension == "CMD")
                extension = "bat";

            if (forceInterpret.Contains(_path + ";")) extension = extension == "DIR" ? "" : "dir";

            BitmapImage elemIcon = new BitmapImage();
            elemIcon.BeginInit();
            elemIcon.UriSource = HasIcon(extension.ToUpper()) ? new Uri("pack://application:,,,/Images/" + extension.ToLower() + "64.png", UriKind.Absolute) : new Uri("pack://application:,,,/Images/file64.png", UriKind.Absolute);
            elemIcon.EndInit();
            return elemIcon;
        }

        private bool HasIcon(string extension) => extension == "DIR" || extension == "AVI" || extension == "CSS" || extension == "DLL" || extension == "DOC" || extension == "DOCX" || extension == "EXE" || extension == "HTML" || extension == "ISO" || extension == "JPG" || extension == "JPEG" || extension == "JS" || extension == "JSON" || extension == "MP3" || extension == "MP4" || extension == "PDF" || extension == "PNG" || extension == "PSD" || extension == "RTF" || extension == "SVG" || extension == "TXT" || extension == "XML" || extension == "ZIP" || extension == "7Z" || extension == "RAR" || extension == "TAR" || extension == "INI" || extension == "JAR" || extension == "BAT" || extension == "SH" || extension == "CMD" || extension == "GIF";

        private void Btn_Back_Click(object sender, RoutedEventArgs e) => Navigate(previousPath[previousPath.Count - 1], true);

        private void ForceInterpret(string _path)
        {
            if (forceInterpret.Contains(_path + ";"))
                forceInterpret = forceInterpret.Replace(_path + ";", "");
            else
                forceInterpret += _path + ";";

            Menu_ExportInterpretData.IsEnabled = forceInterpret.Length > 0;

            ClearTiles();
            Navigate(currentPath);
        }

        private void Menu_ExportInterpretData_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog SFD = new SaveFileDialog
            {
                Title = "Export interpret as a file/folder data",
                RestoreDirectory = true,
                Filter = "Text files|*.txt|All files|*.*",
                DefaultExt = "txt"
            };

            if (SFD.ShowDialog() == true)
                File.WriteAllText(SFD.FileName, forceInterpret);
        }
    }
}
