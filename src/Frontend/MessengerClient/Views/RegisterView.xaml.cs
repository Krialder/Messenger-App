// ========================================
// PSEUDO-CODE - Sprint 7: Register View Code-Behind
// ========================================

using System.Windows.Controls;
using MessengerClient.ViewModels;

namespace SecureMessenger.Client.Views;

public partial class RegisterView : UserControl
{
    public RegisterView()
    {
        InitializeComponent();

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
    }
}
