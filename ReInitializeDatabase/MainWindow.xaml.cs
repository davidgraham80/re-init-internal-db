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
            DataContext = VM;
            Loaded += async (_, __) => await LoadAsync();
        }

        async Task LoadAsync()
        {
            var svc = new  InternalDbService("http://navserver2.navtor.com/ENCSync.svc");







            IReadOnlyList<InternalDBFile> files = await svc.GetFilesAsync(); // returns InternalDBFile[]
            VM.Files.Clear();
            foreach (var f in files)
                VM.Files.Add(new FileChoice { FileName = f.FileName, FileSize = f.FileSize, Source = f });
        }

        void SelectAll_Click(object s, RoutedEventArgs e) => VM.Files.ToList().ForEach(x => x.IsChecked = true);

        async void Ok_Click(object s, RoutedEventArgs e)
        {
            List<InternalDBFile> chosen = VM.Files.Where(x => x.IsChecked).Select(x => x.Source).ToList();
            var svc = new InternalDbService("http://navserver2.navtor.com/ENCSync.svc");
            await svc.SendAsync(chosen);
            MessageBox.Show("Done.");
        }
    }
}
