using ReInitializeDatabase.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SendCheck.ENCSyncClient;
using SendCheck.Poco;
using NavBox.Files;

namespace ReInitializeDatabase
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public FilesVM VM { get; } = new FilesVM();
        public MainWindow()
        {
            InitializeComponent();
            //DataContext = VM;
            DataContext = new MainWindowVm();
            //Loaded += async (_, __) => await LoadAsync();
        }

        async Task LoadAsync()
        {
            var svc = new  InternalDbService("http://navserver2.navtor.com/ENCSync.svc", VM.SerialNumber);







            IReadOnlyList<InternalDBFile> files = await svc.GetFilesAsync(); // returns InternalDBFile[]
            VM.Files.Clear();
            foreach (var f in files)
                VM.Files.Add(new FileChoiceVm { FileName = f.FileName, FileSize = f.FileSize, Source = f });
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (VM.Files.Count == 0)
                return;

            // Determine if we’re mostly checked or unchecked right now
            bool shouldCheck = VM.Files.Any(f => !f.IsChecked);

            foreach (var file in VM.Files)
                file.IsChecked = shouldCheck;
        }


        async void Ok_Click(object s, RoutedEventArgs e)
        {
            List<InternalDBFile> chosen = VM.Files.Where(x => x.IsChecked).Select(x => x.Source).ToList();
            var svc = new InternalDbService("http://navserver2.navtor.com/ENCSync.svc", VM.SerialNumber);
            await svc.SendAsync(chosen);
            MessageBox.Show("Done.");
        }

        private async void LoadFiles_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(VM.SerialNumber))
            {
                MessageBox.Show("Please enter a serial number first.");
                return;
            }

            try
            {
                var svc = new InternalDbService("http://navserver2.navtor.com/ENCSync.svc", VM.SerialNumber);

                IReadOnlyList<InternalDBFile> files = await svc.GetFilesAsync();

                if (files == null || files.Count == 0)
                {
                    MessageBox.Show("No files were returned.\n\n" +
                                    "This may happen if the serial number is invalid.",
                                    "No Files Found",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                VM.Files.Clear();
                foreach (InternalDBFile f in files)
                    VM.Files.Add(new FileChoiceVm { FileName = f.FileName, FileSize = f.FileSize, Source = f });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load files:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            // Collect checked files
            var selectedFiles = VM.Files.Where(f => f.IsChecked).ToList();
            if (!selectedFiles.Any())
            {
                MessageBox.Show("Please select at least one file to send.");
                return;
            }

            if (string.IsNullOrWhiteSpace(VM.SerialNumber))
            {
                MessageBox.Show("Please enter a serial number first.");
                return;
            }

            try
            {
                string navSyncVersion = "4.14.1.1024";

                var svc = new InternalDbService(
                    "http://navserver2.navtor.com/ENCSync.svc",
                    VM.SerialNumber);

                List<InternalDBFile> internalFiles = selectedFiles
                                                     .Select(f => (InternalDBFile)f.Source)
                                                     .ToList();

                await svc.SendAsync(internalFiles);

                MessageBox.Show($"{internalFiles.Count} file(s) sent successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Send failed:\n{ex.Message}");
            }
        }

    }
}
