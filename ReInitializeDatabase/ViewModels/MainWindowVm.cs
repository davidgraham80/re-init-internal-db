using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

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
            this.OnPropertyChanged(nameof(this.HasFiles));
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

        public bool IsSerialNumberValid => !string.IsNullOrWhiteSpace(SerialNumber);

        public bool HasFiles => Files.Count > 0;


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
