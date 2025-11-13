using ReInitializeDatabase.ViewModels;
using SendCheck.ENCSyncClient;
using SendCheck.Poco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using Navtor.Message;
using Newtonsoft.Json;
using ReInitializeDatabase.Utilities;

namespace ReInitializeDatabase
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MessageSendingHelper _messageHelper = new MessageSendingHelper();

        private MainWindowVm _vm { get; }
        public MainWindow()
        {
            InitializeComponent();
            _vm = new MainWindowVm();
            DataContext = _vm;
        }

        async Task LoadAsync()
        {
            var svc = new  InternalDbService("http://navserver2.navtor.com/ENCSync.svc", _vm.SerialNumber);

            IReadOnlyList<InternalDBFile> files = await svc.GetFilesAsync(); // returns InternalDBFile[]
            _vm.Files.Clear();
            foreach (var f in files)
                _vm.Files.Add(new FileChoiceVm { FileName = f.FileName, FileSize = f.FileSize, Source = f });
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.Files.Count == 0)
                return;

            // Determine if we’re mostly checked or unchecked right now
            bool shouldCheck = _vm.Files.Any(f => !f.IsChecked);

            foreach (var file in _vm.Files)
                file.IsChecked = shouldCheck;
        }


        async void Ok_Click(object s, RoutedEventArgs e)
        {
            List<InternalDBFile> chosen = _vm.Files.Where(x => x.IsChecked).Select(x => x.Source).ToList();
            var svc = new InternalDbService("http://navserver2.navtor.com/ENCSync.svc", _vm.SerialNumber);
            await svc.SendAsync(chosen);
            MessageBox.Show("Done.");
        }
        IReadOnlyList<InternalDBFile> _filesDetailsFromServer;

        private async void LoadFiles_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_vm.SerialNumber))
            {
                MessageBox.Show("Please enter a serial number first.");
                return;
            }

            try
            {
                _vm.IsBusy = true;

                var svc = new InternalDbService("http://navserver2.navtor.com/ENCSync.svc", _vm.SerialNumber);

                _filesDetailsFromServer = await svc.GetFilesAsync();

                if(_filesDetailsFromServer == null || _filesDetailsFromServer.Count == 0)
                {
                    MessageBox.Show("No files were returned.\n\n" +
                                    "This may happen if the serial number is invalid.",
                                    "No Files Found",
                                    MessageBoxButton.OK, MessageBoxImage.Information);

                    _vm.Files.Clear();
                    return;
                }

                _vm.Files.Clear();
                foreach(InternalDBFile f in _filesDetailsFromServer)
                    _vm.Files.Add(new FileChoiceVm { FileName = f.FileName, FileSize = f.FileSize, Source = f });
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Failed to load files:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _vm.IsBusy = false;
            }
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            // Collect checked files
            List<FileChoiceVm> selectedFiles = _vm.Files.Where(f => f.IsChecked).ToList();
            if (!selectedFiles.Any())
            {
                MessageBox.Show("Please select at least one file to send.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_vm.SerialNumber))
            {
                MessageBox.Show("Please enter a serial number first.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_vm.MacAddress))
            {
                MessageBox.Show("Please enter a MAC-address first.");
                return;
            }

            try
            {
                List<InternalDBFile> internalFiles = selectedFiles
                                                     .Select(f => (InternalDBFile)f.Source)
                                                     .ToList();

                await _messageHelper.SendViaWcf(_vm.MacAddress, internalFiles, _filesDetailsFromServer);


                MessageBox.Show($"{internalFiles.Count} file(s) sent successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Send failed:\n{ex.Message}");
            }
        }

    }
}
