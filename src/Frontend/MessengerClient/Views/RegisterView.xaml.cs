// ========================================
// PSEUDO-CODE - Sprint 7: Register View Code-Behind
// ========================================

using System;
using System.Reactive;
using System.Windows;
using System.Windows.Controls;
using MessengerClient.ViewModels;
using ReactiveUI;

namespace MessengerClient.Views
{
    public partial class RegisterView : UserControl
    {
        public RegisterView()
        {
            InitializeComponent();
            
            // PasswordBox Binding (nicht direkt möglich in XAML)
            PasswordBox.PasswordChanged += (s, e) =>
            {
                if (DataContext is RegisterViewModel vm)
                {
                    vm.Password = PasswordBox.Password;
                }
            };
            
            ConfirmPasswordBox.PasswordChanged += (s, e) =>
            {
                if (DataContext is RegisterViewModel vm)
                {
                    vm.ConfirmPassword = ConfirmPasswordBox.Password;
                }
            };
            
            // Navigation Events
            if (DataContext is RegisterViewModel viewModel)
            {
                viewModel.RegistrationSuccessful += (s, e) =>
                {
                    // Registration successful message is already shown in UI
                    // Auto-navigate to Login after 2 seconds
                    var timer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = TimeSpan.FromSeconds(2)
                    };
                    timer.Tick += (sender, args) =>
                    {
                        timer.Stop();
                        viewModel.NavigateToLoginCommand.Execute(Unit.Default);
                    };
                    timer.Start();
                };
                
                viewModel.NavigateToLogin += (s, e) =>
                {
                    // Navigate to LoginView
                    var loginView = new LoginView();
                    var window = Window.GetWindow(this);
                    if (window != null)
                    {
                        window.Content = loginView;
                    }
                };
            }
        }
    }
}
