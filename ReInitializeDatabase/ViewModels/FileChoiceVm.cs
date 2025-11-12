using System.Collections.ObjectModel;
using System.ComponentModel;
using WsFile = SendCheck.ENCSyncClient.InternalDBFile;

namespace ReInitializeDatabase.ViewModels
{
    public sealed class FileChoiceVm : INotifyPropertyChanged
    {
        private bool _isChecked;

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked == value) return;
                _isChecked = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
            }
        }



        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        //public object Source { get; set; }
        public WsFile Source { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public sealed class FilesVM
    {
        public ObservableCollection<FileChoiceVm> Files { get; } = new ObservableCollection<FileChoiceVm>();
        public string SerialNumber { get; set; }

    }
}