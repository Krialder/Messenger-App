using System.Windows;
using MessengerClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace MessengerClient
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Get MainViewModel from DI container
            var app = (App)Application.Current;
            if (app.ServiceProvider != null)
            {
                DataContext = app.ServiceProvider.GetRequiredService<MainViewModel>();
            }
        }
    }
}
