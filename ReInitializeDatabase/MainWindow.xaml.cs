using ReInitializeDatabase.Utilities;
using ReInitializeDatabase.ViewModels;
using SendCheck.ENCSyncClient;
using SendCheck.Poco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ReInitializeDatabase
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IReadOnlyList<InternalDbFileManifestItem> _manifest;
        private Guid _runId;
        
        private MessageSendingHelper _messageHelper = new MessageSendingHelper();
        private IReadOnlyList<InternalDBFile> _filesDetailsFromServer;

        private MainWindowVm _vm { get; }
        public MainWindow()
        {
            InitializeComponent();
            _vm = new MainWindowVm();
            DataContext = _vm;
        }

        async Task LoadAsync()
        {
            var svc = new InternalDbService("http://navserver2.navtor.com/ENCSync.svc", _vm.SerialNumber);

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

        private async void LoadFiles_Click(object sender, RoutedEventArgs e)
        {
            string statusMsg = string.Empty;
            _vm.StatusMessage = statusMsg;

            if (string.IsNullOrWhiteSpace(_vm.SerialNumber))
            {
                statusMsg = "Please enter a serial number first.";
                _vm.StatusMessage = statusMsg;
                MessageBox.Show(statusMsg);
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

                _manifest = _filesDetailsFromServer
                            .Select(x => new InternalDbFileManifestItem
                            {
                                FileName = x.FileName,
                                UrlToFile = x.Url,
                                ExpectedFileSize = x.FileSize,
                                Crc = x.Crc
                            }).ToList();

                _runId = Guid.NewGuid();

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

        private async void CancelReinit_Click(object sender, RoutedEventArgs e)
        {
            bool success = await _messageHelper.CancelReinitViaWcf(_vm.MacAddress, 
                                                           (current, total) =>
                                                           {
                                                               string msg = "Cancel Re-Initialize database message created successfully…";
                                                               _vm.StatusMessage = msg;
                                                               MessageBox.Show(msg);
                                                           });
        }


        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            _vm.StatusMessage = string.Empty;
            string statusMsg = string.Empty;

            // Collect checked files
            List<FileChoiceVm> selectedFiles = _vm.Files.Where(f => f.IsChecked).ToList();
            if (!selectedFiles.Any())
            {
                _vm.StatusMessage = "Nothing to send – no files selected.";
                MessageBox.Show("Please select at least one file to send.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_vm.SerialNumber))
            {
                statusMsg = "Please enter a serial number first.";
                _vm.StatusMessage = statusMsg;
                MessageBox.Show(statusMsg);
                return;
            }

            if (string.IsNullOrWhiteSpace(_vm.MacAddress))
            {
                statusMsg = "Please enter a MAC-address first.";
                _vm.StatusMessage = "Please enter a MAC-address first.";
                MessageBox.Show(statusMsg);
                return;
            }

            try
            {
                List<InternalDBFile> internalFiles = selectedFiles
                                                     .Select(f => (InternalDBFile)f.Source)
                                                     .ToList();

                foreach(InternalDbFileManifestItem manifestItem in _manifest)
                {
                    InternalDBFile isCheckedItem = internalFiles.FirstOrDefault(x => x.FileName == manifestItem.FileName);
                    if(isCheckedItem != null)
                    {
                        InternalDbFileManifestItem manifestToUpdate = _manifest.FirstOrDefault(m => m.FileName == isCheckedItem.FileName);
                        if(manifestToUpdate != null)
                        {
                            manifestToUpdate.Sent = true;
                        }
                        else
                        {
                            manifestItem.Sent = false;
                        }
                    }
                    else
                    {
                        manifestItem.Sent = false;
                    }
                }

                bool success = await _messageHelper.SendViaWcf(_manifest, _vm.MacAddress, internalFiles, _filesDetailsFromServer,
                                                               (current, total) =>
                                                               {
                                                                   _vm.StatusMessage = $"Sending message {current} of {total}…";
                                                               }, _vm.SkipChartUpdate);

                if(success)
                {
                    statusMsg = $"{internalFiles.Count} file(s) sent successfully.";
                    _vm.StatusMessage = statusMsg;
                    MessageBox.Show(statusMsg);
                }
                else
                {
                    statusMsg = string.Empty;
                    _vm.StatusMessage = statusMsg;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Send failed:\n{ex.Message}");
            }
        }

    }
}
