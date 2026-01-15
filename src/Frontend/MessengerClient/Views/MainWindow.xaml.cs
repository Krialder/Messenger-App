using System.Windows;
using MessengerClient.ViewModels;

namespace MessengerClient.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(MainViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
