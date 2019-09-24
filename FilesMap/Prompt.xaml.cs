using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using Microsoft.Win32;
using FilesMap.Properties;

namespace FilesMap
{
    public partial class Prompt : Window
    {
        private readonly MainWindow Main;

        private bool manualClose = true;

        private string forceInterpretData = "";

        public Prompt(MainWindow mainW)
        {
            InitializeComponent();
            Main = mainW;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => DialogResult = !manualClose;

        private void Btn_OK_Click(object sender, RoutedEventArgs e)
        {
            if (Btn_OK.Content.ToString() == "Save")
            {
                if (Txt_DriveSeparator.Text == Txt_DirSeparator.Text || Txt_DriveSeparator.Text == Txt_FileExtSeparator.Text || Txt_FileExtSeparator.Text == Txt_DirSeparator.Text)
                {
                    MessageBox.Show("A separator value can't be the same than another one.", "FilesMap", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                Grid_Settings.Visibility = Visibility.Hidden;
                if (Txt_DriveSeparator.Text.Length > 0) Settings.Default.DriveSeparator = Txt_DriveSeparator.Text;
                if (Txt_DirSeparator.Text.Length > 0) Settings.Default.DirSeparator = Txt_DirSeparator.Text;
                if (Txt_FileExtSeparator.Text.Length > 0) Settings.Default.FileExtSeparator = Txt_FileExtSeparator.Text;
                Settings.Default.Save();
                Btn_Settings.Visibility = Visibility.Visible;
                Btn_Import.Visibility = Visibility.Visible;
                Rtxt_Data.Visibility = Visibility.Visible;
                Lbl_Title.Content = "Insert data:";
                Btn_OK.Content = "OK";
                return;
            }

            string driveSeparator = Settings.Default.DriveSeparator;

            string[] a = new TextRange(Rtxt_Data.Document.ContentStart, Rtxt_Data.Document.ContentEnd).Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            if (a.Length < 1)
            {
                MessageBox.Show("Data can't be null.", "FilesMap", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            string drive = "C" + driveSeparator;
            foreach (string element in a)
            {
                if (element.Contains(driveSeparator))
                {
                    drive = element.Remove(element.IndexOf(driveSeparator) + 1, element.Length - element.IndexOf(driveSeparator) - 1);
                    break;
                }
            }

            Main.data = a;
            Main.defaultDrive = drive;
            Main.forceInterpret = forceInterpretData;
            Main.Navigate(drive);
            manualClose = false;
            Close();
        }

        private void Btn_Settings_Click(object sender, RoutedEventArgs e)
        {
            Btn_Settings.Visibility = Visibility.Hidden;
            Btn_Import.Visibility = Visibility.Hidden;
            Rtxt_Data.Visibility = Visibility.Hidden;
            Lbl_Title.Content = "Settings";
            Btn_OK.Content = "Save";
            Txt_DriveSeparator.Text = Settings.Default.DriveSeparator;
            Txt_DirSeparator.Text = Settings.Default.DirSeparator;
            Txt_FileExtSeparator.Text = Settings.Default.FileExtSeparator;
            CheckBox_DeleteConfirmation.IsChecked = Settings.Default.deleteConfirmation;
            Grid_Settings.Visibility = Visibility.Visible;
        }

        private void Btn_ImportForceInterpretData_Click(object sender, RoutedEventArgs e)
        {
            string file = ShowOpenDialog("Import interpret as a file/folder data");

            if (file.Length > 0)
            {
                forceInterpretData = File.ReadAllText(file);
                string fileName = file.Remove(0, file.LastIndexOf("\\") + 1);
                Lbl_ForceInterpretData.Content = "Using " + (fileName.Length > 25 ? fileName.Remove(24) + "..." : fileName);
            }
        }

        private void Btn_Import_Click(object sender, RoutedEventArgs e)
        {
            string file = ShowOpenDialog("Import data");

            if (file.Length > 0)
            {
                Rtxt_Data.Document.Blocks.Clear();
                string fileData = File.ReadAllText(file);
                foreach (string line in new LineReader(() => new StringReader(fileData)))
                {
                    Rtxt_Data.Document.Blocks.Add(new Paragraph(new Run(line)));
                }
            }
        }

        private string ShowOpenDialog(string title)
        {
            OpenFileDialog OFD = new OpenFileDialog
            {
                Title = title,
                RestoreDirectory = true,
                Filter = "Text files|*.txt|All files|*.*",
                DefaultExt = "txt"
            };

            return OFD.ShowDialog() == true ? OFD.FileName : "";
        }

        private void CheckBox_DeleteConfirmation_CheckedChanged(object sender, RoutedEventArgs e) => Settings.Default.deleteConfirmation = CheckBox_DeleteConfirmation.IsChecked == true;
    }
}
