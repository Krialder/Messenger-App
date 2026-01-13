using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using MessengerClient.Services;
using MessengerContracts.DTOs;

namespace MessengerClient.ViewModels
{
    /// <summary>
    /// LoginViewModel - Handles user authentication with MFA support
    /// Implements password-based login, TOTP/YubiKey verification, and master key derivation
    /// Follows ReactiveUI MVVM pattern with observable properties and reactive commands
    /// </summary>
    public class LoginViewModel : ReactiveObject
    {
        private readonly IAuthApiService _authApi;
        private readonly SignalRService _signalR;
        private readonly LocalStorageService _localStorage;
        private readonly LocalCryptoService _crypto;

        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;
        private bool _mfaRequired;
        private string _mfaCode = string.Empty;
        private string? _mfaSessionToken;

        /// <summary>
        /// User's email for login
        /// </summary>
        public string Email
        {
            get => _email;
            set => this.RaiseAndSetIfChanged(ref _email, value);
        }

        /// <summary>
        /// User's password for login
        /// </summary>
        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        /// <summary>
        /// Error message to display on login failure
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        /// <summary>
        /// Indicates if the login or MFA verification is in progress
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        /// <summary>
        /// Indicates if MFA is required for login
        /// </summary>
        public bool MfaRequired
        {
            get => _mfaRequired;
            set => this.RaiseAndSetIfChanged(ref _mfaRequired, value);
        }

        /// <summary>
        /// MFA code entered by the user for verification
        /// </summary>
        public string MfaCode
        {
            get => _mfaCode;
            set => this.RaiseAndSetIfChanged(ref _mfaCode, value);
        }

        /// <summary>
        /// Command to execute login asynchronously
        /// </summary>
        public ReactiveCommand<Unit, Unit> LoginCommand { get; }

        /// <summary>
        /// Command to execute MFA verification asynchronously
        /// </summary>
        public ReactiveCommand<Unit, Unit> VerifyMfaCommand { get; }

        /// <summary>
        /// Command to navigate to the registration page
        /// </summary>
        public ReactiveCommand<Unit, Unit> NavigateToRegisterCommand { get; }

        /// <summary>
        /// Event triggered when login is successful
        /// </summary>
        public event EventHandler? LoginSuccessful;

        /// <summary>
        /// Event triggered to navigate to the registration page
        /// </summary>
        public event EventHandler? NavigateToRegister;

        /// <summary>
        /// LoginViewModel constructor
        /// Initializes commands and sets up reactive property triggers
        /// </summary>
        /// <param name="authApi">Authentication API service</param>
        /// <param name="signalR">SignalR service for real-time messaging</param>
        /// <param name="localStorage">Local storage service for persisting data</param>
        /// <param name="crypto">Crypto service for password and data encryption</param>
        public LoginViewModel(
            IAuthApiService authApi,
            SignalRService signalR,
            LocalStorageService localStorage,
            LocalCryptoService crypto)
        {
            _authApi = authApi;
            _signalR = signalR;
            _localStorage = localStorage;
            _crypto = crypto;

            // Enable login command only when email and password are provided
            IObservable<bool> canLogin = this.WhenAnyValue(
                x => x.Email,
                x => x.Password,
                x => x.IsLoading,
                (email, password, loading) => 
                    !string.IsNullOrWhiteSpace(email) && 
                    !string.IsNullOrWhiteSpace(password) && 
                    !loading);

            LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync, canLogin);

            // Enable MFA verification only when code is entered
            IObservable<bool> canVerifyMfa = this.WhenAnyValue(
                x => x.MfaCode,
                x => x.IsLoading,
                (code, loading) => !string.IsNullOrWhiteSpace(code) && !loading);

            VerifyMfaCommand = ReactiveCommand.CreateFromTask(VerifyMfaAsync, canVerifyMfa);

            NavigateToRegisterCommand = ReactiveCommand.Create(() =>
            {
                NavigateToRegister?.Invoke(this, EventArgs.Empty);
            });
        }

        /// <summary>
        /// Step 1: Authenticate with username/password
        /// If MFA is enabled, server responds with MfaRequired flag
        /// </summary>
        private async Task LoginAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                LoginRequest request = new LoginRequest(Email, Password);

                LoginResponse response = await _authApi.LoginAsync(request);

                await HandleSuccessfulLoginAsync(response);
            }
            catch (Refit.ApiException ex)
            {
                ErrorMessage = $"Login failed: {ex.Content ?? ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Step 2: Verify MFA code (TOTP, YubiKey, or Recovery Code)
        /// Called after initial login if MFA is enabled
        /// </summary>
        private async Task VerifyMfaAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                VerifyMfaRequest request = new VerifyMfaRequest(Guid.Empty, MfaCode);

                LoginResponse response = await _authApi.VerifyMfaAsync(request);
                await HandleSuccessfulLoginAsync(response);
            }
            catch (Refit.ApiException ex)
            {
                ErrorMessage = $"MFA verification failed: {ex.Content ?? ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"MFA verification failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Post-authentication: Derive master key and initialize local crypto
        /// Master key derivation: Argon2id(password, userSalt)
        /// Master key stays in RAM only (cleared on logout)
        /// </summary>
        private async Task HandleSuccessfulLoginAsync(LoginResponse response)
        {
            // Validate response
            if (response?.User == null)
            {
                ErrorMessage = "Invalid server response";
                return;
            }

            // Save JWT access token for API authentication
            await _localStorage.SaveTokenAsync(response.AccessToken);

            // Generate salt for master key derivation
            byte[] salt = _crypto.GenerateSalt();
            
            // Derive master key from password (Layer 2 encryption)
            byte[] masterKey = await _crypto.DeriveMasterKeyAsync(Password, salt);

            // Save encrypted user profile locally
            await _localStorage.SaveUserProfileAsync(
                response.User.Id,
                response.User.Email,
                response.User.Username,
                salt,
                Array.Empty<byte>()
            );

            // Establish SignalR connection for real-time messaging
            bool signalRConnected = await _signalR.ConnectAsync(response.AccessToken);
            if (!signalRConnected)
            {
                // Log warning but don't block login - user can still use app offline
                Console.WriteLine("Warning: SignalR connection failed - real-time features unavailable");
            }

            // Notify UI to navigate to main window
            LoginSuccessful?.Invoke(this, EventArgs.Empty);
        }
    }
}
