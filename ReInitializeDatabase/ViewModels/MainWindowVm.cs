using System.ComponentModel;

namespace ReInitializeDatabase.ViewModels
{
    public class MainWindowVm : INotifyPropertyChanged
    {
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
            }
        }

        public bool IsSerialNumberValid => !string.IsNullOrWhiteSpace(SerialNumber);

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
