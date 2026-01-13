# 🚀 Quick Start Guide - XAML Implementation (Phase 13.1)

## 📋 **Aktuelle Situation** (Version 8.0)

✅ **COMPLETE** (80%):
- Backend Services (9/9) - 100% Tested
- Frontend Backend-Logik (Services, ViewModels, Database) - 100%
- API Integration (Refit + SignalR) - 100%
- Encryption Integration (Layer 1 + Layer 2) - 100%

⏳ **PENDING** (20%):
- XAML Views (7 Views)
- Value Converters (10 Converters)
- Resource Dictionaries (3 Files)

**Estimated Time**: **8-12 hours** (Pure XAML work)

---

## 🎯 **Nächster Schritt: XAML Views implementieren**

### **Reihenfolge der Implementierung**

1. **LoginView.xaml** (1-2 Stunden) - START HERE
2. **MainWindow.xaml** (1-2 Stunden) - Navigation Shell
3. **ChatView.xaml** (3-4 Stunden) - Most Complex
4. **RegisterView.xaml** (1 Stunde)
5. **ContactsView.xaml** (1 Stunde)
6. **SettingsView.xaml** (1 Stunde)
7. **MFASetupView.xaml** (30 Minuten)
8. **Value Converters** (1 Stunde) - 10 Converters
9. **Resource Dictionaries** (1 Stunde) - Colors, Styles

---

## 🛠️ **Step 1: LoginView.xaml** (START HERE)

### **File**: `src/Frontend/MessengerClient/Views/LoginView.xaml`

```xml
<UserControl x:Class="MessengerClient.Views.LoginView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes"
             xmlns:viewModels="clr-namespace:MessengerClient.ViewModels"
             d:DataContext="{d:DesignInstance Type=viewModels:LoginViewModel}">
    
    <Grid Background="{DynamicResource MaterialDesignPaper}">
        <StackPanel HorizontalAlignment="Center" VerticalAlignment="Center" Width="400">
            
            <!-- Logo -->
            <materialDesign:PackIcon Kind="MessageLock" Width="100" Height="100" 
                                     HorizontalAlignment="Center" 
                                     Foreground="{DynamicResource PrimaryHueMidBrush}"
                                     Margin="0,0,0,20"/>
            
            <!-- Title -->
            <TextBlock Text="Secure Messenger" 
                      FontSize="32" 
                      FontWeight="Bold" 
                      HorizontalAlignment="Center" 
                      Margin="0,0,0,40"
                      Foreground="{DynamicResource MaterialDesignBody}"/>
            
            <!-- Email Input -->
            <TextBox x:Name="EmailTextBox"
                     materialDesign:HintAssist.Hint="E-Mail" 
                     Text="{Binding Email, UpdateSourceTrigger=PropertyChanged}"
                     Style="{StaticResource MaterialDesignOutlinedTextBox}"
                     Margin="0,0,0,20"/>
            
            <!-- Password Input -->
            <PasswordBox x:Name="PasswordBox"
                        materialDesign:HintAssist.Hint="Passwort"
                        Style="{StaticResource MaterialDesignOutlinedPasswordBox}"
                        Margin="0,0,0,20"/>
            
            <!-- MFA Input (Visible only if MfaRequired) -->
            <TextBox materialDesign:HintAssist.Hint="MFA Code (6 Ziffern)" 
                     Text="{Binding MfaCode, UpdateSourceTrigger=PropertyChanged}"
                     Style="{StaticResource MaterialDesignOutlinedTextBox}"
                     Visibility="{Binding MfaRequired, Converter={StaticResource BoolToVisibilityConverter}}"
                     MaxLength="6"
                     Margin="0,0,0,20"/>
            
            <!-- Error Message -->
            <TextBlock Text="{Binding ErrorMessage}" 
                      Foreground="#F44336"
                      TextWrapping="Wrap"
                      Margin="0,0,0,10"
                      Visibility="{Binding ErrorMessage, Converter={StaticResource StringToVisibilityConverter}}"/>
            
            <!-- Loading Indicator -->
            <ProgressBar IsIndeterminate="True"
                        Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisibilityConverter}}"
                        Margin="0,0,0,20"/>
            
            <!-- Login Button -->
            <Button Content="ANMELDEN" 
                   Command="{Binding LoginCommand}"
                   Style="{StaticResource MaterialDesignRaisedButton}"
                   Height="40"
                   Margin="0,20,0,10"
                   Visibility="{Binding MfaRequired, Converter={StaticResource InverseBoolToVisibilityConverter}}"/>
            
            <!-- Verify MFA Button -->
            <Button Content="MFA CODE VERIFIZIEREN" 
                   Command="{Binding VerifyMfaCommand}"
                   Style="{StaticResource MaterialDesignRaisedButton}"
                   Height="40"
                   Margin="0,20,0,10"
                   Visibility="{Binding MfaRequired, Converter={StaticResource BoolToVisibilityConverter}}"/>
            
            <!-- Register Link -->
            <Button Content="Neuen Account erstellen" 
                   Command="{Binding NavigateToRegisterCommand}"
                   Style="{StaticResource MaterialDesignFlatButton}"
                   Margin="0,10,0,0"/>
            
        </StackPanel>
    </Grid>
</UserControl>
```

### **Code-Behind**: `src/Frontend/MessengerClient/Views/LoginView.xaml.cs`

```csharp
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
            
            // PasswordBox Binding (nicht direkt möglich in XAML)
            PasswordBox.PasswordChanged += (s, e) =>
            {
                if (DataContext is LoginViewModel vm)
                {
                    vm.Password = PasswordBox.Password;
                }
            };
            
            // Navigation Events
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.LoginSuccessful += (s, e) =>
                {
                    // Navigate to MainWindow
                    var mainWindow = new MainWindow
                    {
                        DataContext = Application.Current.MainWindow?.DataContext
                    };
                    mainWindow.Show();
                    Window.GetWindow(this)?.Close();
                };
                
                viewModel.NavigateToRegister += (s, e) =>
                {
                    // Navigate to RegisterView
                    var registerView = new RegisterView();
                    Window.GetWindow(this)?.Content = registerView;
                };
            }
        }
    }
}
```

---

## 🛠️ **Step 2: Value Converters erstellen**

### **File**: `src/Frontend/MessengerClient/Converters/BoolToVisibilityConverter.cs`

```csharp
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MessengerClient.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }
}
```

### **File**: `src/Frontend/MessengerClient/Converters/InverseBoolToVisibilityConverter.cs`

```csharp
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MessengerClient.Converters
{
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
```

### **File**: `src/Frontend/MessengerClient/Converters/StringToVisibilityConverter.cs`

```csharp
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MessengerClient.Converters
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return string.IsNullOrWhiteSpace(str) ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
```

---

## 🛠️ **Step 3: App.xaml - Resource Dictionaries registrieren**

### **File**: `src/Frontend/MessengerClient/App.xaml`

```xml
<Application x:Class="MessengerClient.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes"
             xmlns:converters="clr-namespace:MessengerClient.Converters">
    
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- MaterialDesign Theme -->
                <materialDesign:BundledTheme BaseTheme="Dark" PrimaryColor="Blue" SecondaryColor="Pink"/>
                <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml"/>
            </ResourceDictionary.MergedDictionaries>
            
            <!-- Converters -->
            <converters:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter"/>
            <converters:InverseBoolToVisibilityConverter x:Key="InverseBoolToVisibilityConverter"/>
            <converters:StringToVisibilityConverter x:Key="StringToVisibilityConverter"/>
            
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

---

## 🧪 **Step 4: Testen**

### **Backend Services starten**

```sh
# Terminal 1: Gateway
cd src/Backend/GatewayService
dotnet run

# Terminal 2: AuthService
cd src/Backend/AuthService
dotnet run

# Terminal 3: MessageService
cd src/Backend/MessageService
dotnet run

# (Optional: Alle Services mit docker-compose up -d)
```

### **Frontend starten**

```sh
# Visual Studio:
# - Set MessengerClient as Startup Project
# - Press F5

# ODER Terminal:
cd src/Frontend/MessengerClient
dotnet run
```

### **Test-Szenarien**

1. **Login Flow**
   - Email eingeben: `test@example.com`
   - Password eingeben: `Test1234!`
   - Login Button klicken
   - → JWT Token wird gespeichert
   - → Navigate to MainWindow

2. **MFA Flow** (falls aktiviert)
   - Login → MfaRequired = true
   - MFA Code Input erscheint
   - Code eingeben (6 Ziffern)
   - Verify Button → JWT Token

3. **Error Handling**
   - Falsches Passwort → ErrorMessage wird angezeigt
   - Leere Felder → Login Button disabled (CanExecute)

---

## 📚 **Nächste Views**

Nach `LoginView.xaml`:

1. **MainWindow.xaml** - Navigation Shell
2. **ChatView.xaml** - Chat UI (Complex!)
3. **RegisterView.xaml** - Registration Form
4. **ContactsView.xaml** - Contact List
5. **SettingsView.xaml** - Settings Panel

**Siehe**: `README_IMPLEMENTATION.md` für vollständige XAML-Vorlagen

---

## 🎯 **Ziel**

**Nach Abschluss von Phase 13.1**:
- ✅ Alle 7 XAML Views implementiert
- ✅ Alle Value Converters erstellt
- ✅ Resource Dictionaries konfiguriert
- ✅ Frontend 100% Complete
- ✅ E2E Tests (Phase 13.2)
- ✅ Production Ready!

**Geschätzte Zeit**: **8-12 Stunden** (reine XAML-Arbeit)

---

**Version**: 8.0  
**Last Updated**: 2025-01-10  
**Status**: 🟡 **Ready for XAML Implementation**

**Start Here**: `LoginView.xaml` → `MainWindow.xaml` → `ChatView.xaml`
