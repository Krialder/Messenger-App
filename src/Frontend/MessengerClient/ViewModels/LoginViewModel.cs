using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MessengerClient.Commands;
using MessengerClient.Services;
using MessengerContracts.DTOs;
using ContractsLoginResponse = MessengerContracts.DTOs.LoginResponse;
using ContractsLoginRequest = MessengerContracts.DTOs.LoginRequest;

namespace MessengerClient.ViewModels
{
    /// <summary>
    /// LoginViewModel - Handles user authentication with MFA support
    /// Implements password-based login, TOTP/YubiKey verification, and master key derivation
    /// Follows ReactiveUI MVVM pattern with observable properties and reactive commands
    /// </summary>
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IAuthApiService _authApiService;
        private readonly ITokenStorageService _tokenStorage;
        
        // Events
        public event EventHandler? LoginSuccessful;
        public event EventHandler? NavigateToRegister;
        
        // Properties
        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }
        
        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        
        private string _mfaCode = string.Empty;
        public string MfaCode
        {
            get => _mfaCode;
            set => SetProperty(ref _mfaCode, value);
        }
        
        private bool _mfaRequired = false;
        public bool MfaRequired
        {
            get => _mfaRequired;
            set => SetProperty(ref _mfaRequired, value);
        }
        
        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        
        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }
        
        public bool CanLogin => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password) && !IsLoading;
        public bool CanVerifyMfa => !string.IsNullOrWhiteSpace(MfaCode) && MfaCode.Length == 6 && !IsLoading;
        
        // Commands
        public ICommand LoginCommand { get; }
        public ICommand VerifyMfaCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }
        
        public LoginViewModel(IAuthApiService authApiService, ITokenStorageService tokenStorage)
        {
            _authApiService = authApiService;
            _tokenStorage = tokenStorage;
            LoginCommand = new RelayCommand(async () => await LoginAsync(), () => CanLogin);
            VerifyMfaCommand = new RelayCommand(async () => await VerifyMfaAsync(), () => CanVerifyMfa);
            NavigateToRegisterCommand = new RelayCommand(() => NavigateToRegister?.Invoke(this, EventArgs.Empty));
        }
        
        private async Task LoginAsync()
        {
            try
            {
                ErrorMessage = string.Empty;
                IsLoading = true;
                
                var request = new ContractsLoginRequest(Email, Password);
                var response = await _authApiService.LoginAsync(request);
                
                if (response.MfaRequired)
                {
                    // MFA required - save email for MFA verification
                    MfaRequired = true;
                    ErrorMessage = string.Empty;
                }
                else
                {
                    // Login successful without MFA
                    if (!string.IsNullOrEmpty(response.AccessToken) && !string.IsNullOrEmpty(response.RefreshToken))
                    {
                        await _tokenStorage.StoreTokensAsync(response.AccessToken, response.RefreshToken);
                        LoginSuccessful?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        ErrorMessage = "Invalid response from server";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
                MfaRequired = false;
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async Task VerifyMfaAsync()
        {
            try
            {
                ErrorMessage = string.Empty;
                IsLoading = true;
                
                var request = new VerifyMfaRequest(Email, MfaCode);
                var response = await _authApiService.VerifyMfaAsync(request);
                
                // Store tokens securely
                if (!string.IsNullOrEmpty(response.AccessToken) && !string.IsNullOrEmpty(response.RefreshToken))
                {
                    await _tokenStorage.StoreTokensAsync(response.AccessToken, response.RefreshToken);
                    LoginSuccessful?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ErrorMessage = "Invalid response from server";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"MFA verification failed: {ex.Message}";
                MfaCode = string.Empty; // Clear invalid code
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            
            // Notify command can execute changed
            if (propertyName is nameof(Email) or nameof(Password) or nameof(IsLoading))
            {
                OnPropertyChanged(nameof(CanLogin));
            }
            
            if (propertyName is nameof(MfaCode) or nameof(IsLoading))
            {
                OnPropertyChanged(nameof(CanVerifyMfa));
            }
            
            return true;
        }
    }
}
