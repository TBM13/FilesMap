using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FilesMap
{
    public partial class MainWindow : Window
    {
        public string[] data;

        private double previousWidth = 0;

        private int tilesCount;
        private int column = 0;

        private string currentPath;

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

            string name = _path.Remove(0, _path.LastIndexOf("\\") + 1);
            string extension = _path.Contains(".") ? _path.Remove(0, _path.LastIndexOf(".") + 1) : "";

            Grid tileBackground = new Grid
            {
                ToolTip = name,
                Width = 64,
                Height = 64,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(a, b, 0, 0)
            };

            Image tile = new Image
            {
                Width = 48,
                Height = 48,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                //Source = GetFileIcon(extension),
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

        public void Navigate(string _path)
        {
            try
            {
                ClearTiles();
                foreach (string element in data)
                {
                    string elem = element;
                    if (elem.Substring(elem.Length - 1) == "\\") elem = elem.Remove(elem.Length - 1, 1);
                    if (_path.Substring(_path.Length - 1) != "\\") _path += "\\";
                    if (elem.Contains(_path) && elem.Length - elem.Replace("\\", "").Length == _path.Length - _path.Replace("\\", "").Length)
                    {
                        AddTile(elem);
                    }
                }
                currentPath = _path;
                Textbox_Path.Text = _path;
            }
            catch (Exception e)
            {
                MessageBox.Show("Se ha producido un error:\n" + e.ToString(), "FilesMap", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

#pragma warning disable IDE0060
        private void TileMouseUp(object sender, MouseButtonEventArgs e, Image tile, string _path)
        {
            if (e.ChangedButton == MouseButton.Left && !_path.Contains(".")) Navigate(_path);
        }

        private void TileMouseEnter(object sender, MouseEventArgs e, Grid tileBackground) => tileBackground.Background = Brushes.LightBlue;
        private void TileMouseLeave(object sender, MouseEventArgs e, Grid tileBackground) => tileBackground.Background = Brushes.Transparent;

        private void Button_Click(object sender, RoutedEventArgs e) => Navigate(Textbox_Path.Text);
#pragma warning restore IDE0060
    }
}
