// ========================================
// PSEUDO-CODE - Sprint 7: Login View Code-Behind
// Status: 🔶 MVVM Pattern
// ========================================

using System.Windows.Controls;

namespace SecureMessenger.Client.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
        
        // PSEUDO: DataContext will be set by DI container
        // DataContext = App.Current.Services.GetService<LoginViewModel>();
    }
}
