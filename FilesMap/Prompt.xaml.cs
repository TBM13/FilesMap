using System;
using System.Windows;
using System.Windows.Documents;

namespace FilesMap
{
    public partial class Prompt : Window
    {
        private readonly MainWindow Main;

        private bool manualClose = true;

        public Prompt(MainWindow mainW)
        {
            InitializeComponent();
            Main = mainW;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => DialogResult = !manualClose;

        private void Btn_OK_Click(object sender, RoutedEventArgs e)
        {
            string[] a = new TextRange(Rtxt_Data.Document.ContentStart, Rtxt_Data.Document.ContentEnd).Text.Replace("/", "\\").Split(new[]{ Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            if (a.Length < 1)
            {
                MessageBox.Show("Data can't be null.", "FilesMap", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            Main.data = a;
            Main.Navigate("C:\\");
            manualClose = false;
            Close();
        }
    }
}
