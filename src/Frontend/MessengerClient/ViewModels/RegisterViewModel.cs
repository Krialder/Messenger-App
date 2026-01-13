using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using MessengerClient.Services;
using MessengerContracts.DTOs;

namespace MessengerClient.ViewModels
{
    public class RegisterViewModel : ReactiveObject
    {
        private readonly IAuthApiService _authApi;
        private readonly LocalStorageService _localStorage;
        private readonly LocalCryptoService _crypto;

        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _displayName = string.Empty;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private bool _isLoading;

        public string Email
        {
            get => _email;
            set => this.RaiseAndSetIfChanged(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => this.RaiseAndSetIfChanged(ref _confirmPassword, value);
        }

        public string DisplayName
        {
            get => _displayName;
            set => this.RaiseAndSetIfChanged(ref _displayName, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public string SuccessMessage
        {
            get => _successMessage;
            set => this.RaiseAndSetIfChanged(ref _successMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public ReactiveCommand<Unit, Unit> RegisterCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToLoginCommand { get; }

        public event EventHandler? RegistrationSuccessful;
        public event EventHandler? NavigateToLogin;

        public RegisterViewModel(
            IAuthApiService authApi,
            LocalStorageService localStorage,
            LocalCryptoService crypto)
        {
            _authApi = authApi;
            _localStorage = localStorage;
            _crypto = crypto;

            IObservable<bool> canRegister = this.WhenAnyValue(
                x => x.Email,
                x => x.Password,
                x => x.ConfirmPassword,
                x => x.DisplayName,
                x => x.IsLoading,
                (email, password, confirmPassword, displayName, loading) =>
                    !string.IsNullOrWhiteSpace(email) &&
                    !string.IsNullOrWhiteSpace(password) &&
                    !string.IsNullOrWhiteSpace(confirmPassword) &&
                    !string.IsNullOrWhiteSpace(displayName) &&
                    password == confirmPassword &&
                    password.Length >= 8 &&
                    !loading);

            RegisterCommand = ReactiveCommand.CreateFromTask(RegisterAsync, canRegister);

            NavigateToLoginCommand = ReactiveCommand.Create(() =>
            {
                NavigateToLogin?.Invoke(this, EventArgs.Empty);
            });
        }

        private async Task RegisterAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            try
            {
                if (Password != ConfirmPassword)
                {
                    ErrorMessage = "Passwords do not match";
                    return;
                }

                if (Password.Length < 8)
                {
                    ErrorMessage = "Password must be at least 8 characters";
                    return;
                }

                byte[] salt = _crypto.GenerateSalt();

                RegisterRequest request = new RegisterRequest(DisplayName, Email, Password);

                RegisterResponse response = await _authApi.RegisterAsync(request);

                byte[] masterKeySalt = Convert.FromBase64String(response.MasterKeySalt);

                await _localStorage.SaveUserProfileAsync(
                    response.UserId,
                    response.Email,
                    response.Username,
                    masterKeySalt,
                    Array.Empty<byte>()
                );

                SuccessMessage = "Registration successful! Redirecting to login...";

                await Task.Delay(2000);
                RegistrationSuccessful?.Invoke(this, EventArgs.Empty);
            }
            catch (Refit.ApiException ex)
            {
                ErrorMessage = $"Registration failed: {ex.Content ?? ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Registration failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
