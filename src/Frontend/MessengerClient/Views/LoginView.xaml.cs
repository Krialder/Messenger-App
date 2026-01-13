// ========================================
// PSEUDO-CODE - Sprint 7: Login View Code-Behind
// Status: 🔶 MVVM Pattern
// ========================================

using System.Windows;
using System.Windows.Controls;
using MessengerClient.ViewModels;

namespace MessengerClient.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
            
            PasswordBox.PasswordChanged += (s, e) =>
            {
                if (DataContext is LoginViewModel vm)
                {
                    vm.Password = PasswordBox.Password;
                }
            };
            
            DataContextChanged += (s, e) =>
            {
                if (DataContext is LoginViewModel viewModel)
                {
                    viewModel.LoginSuccessful += (sender, args) =>
                    {
                        Window? window = Window.GetWindow(this);
                        if (window != null)
                        {
                            MainWindow mainWindow = new MainWindow();
                            mainWindow.Show();
                            window.Close();
                        }
                    };
                }
            };
        }
    }
}
