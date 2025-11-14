using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace ReInitializeDatabase.ViewModels
{
    public class MainWindowVm : INotifyPropertyChanged
    {
        public MainWindowVm()
        {
            Files.CollectionChanged += new NotifyCollectionChangedEventHandler(Files_CollectionChanged);
        }

        private void Files_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Subscribe to IsChecked changes on new items
            if (e.NewItems != null)
            {
                foreach (FileChoiceVm item in e.NewItems)
                    item.PropertyChanged += FilePropertyChanged;
            }

            // Unsubscribe when items are removed
            if (e.OldItems != null)
            {
                foreach (FileChoiceVm item in e.OldItems)
                    item.PropertyChanged -= FilePropertyChanged;
            }

            this.OnPropertyChanged(nameof(this.HasFiles));
        }

        private void FilePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FileChoiceVm.IsChecked))
                this.OnPropertyChanged(nameof(this.CanSend));
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
                OnPropertyChanged(nameof(IsLoadEnabled));
            }
        }

        public bool IsLoadEnabled => IsSerialNumberValid && !IsBusy;

        private string _serialNumber;
        public string SerialNumber
        {
            get => _serialNumber;
            set
            {
                if (_serialNumber == value) return;
                _serialNumber = value;
                OnPropertyChanged(nameof(SerialNumber));
                OnPropertyChanged(nameof(IsSerialNumberValid));
                OnPropertyChanged(nameof(IsLoadEnabled));
            }
        }

        private string _macAddress;
        public string MacAddress
        {
            get => _macAddress;
            set
            {
                if (_macAddress == value) return;
                _macAddress = value;
                OnPropertyChanged(nameof(MacAddress));
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage == value) return;
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public bool IsSerialNumberValid => !string.IsNullOrWhiteSpace(SerialNumber);

        public bool HasFiles => Files.Count > 0;

        public bool CanSend => Files.Any(f => f.IsChecked);


        public ObservableCollection<FileChoiceVm> Files { get; } = new ObservableCollection<FileChoiceVm>();
        
        public void CheckUncheckAll(bool isChecked)
        {
            foreach (FileChoiceVm f in Files) f.IsChecked = isChecked;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
