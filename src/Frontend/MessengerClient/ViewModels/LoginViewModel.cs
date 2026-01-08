using ReactiveUI;
using System.Reactive;

namespace MessengerClient.ViewModels
{
    public class LoginViewModel : ReactiveObject
    {
        // PSEUDO CODE: Login ViewModel (MVVM Pattern)

        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;
        private bool _mfaRequired;
        private string _mfaCode = string.Empty;

        public string Username
        {
            get => _username;
            set => this.RaiseAndSetIfChanged(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public bool MfaRequired
        {
            get => _mfaRequired;
            set => this.RaiseAndSetIfChanged(ref _mfaRequired, value);
        }

        public string MfaCode
        {
            get => _mfaCode;
            set => this.RaiseAndSetIfChanged(ref _mfaCode, value);
        }

        public ReactiveCommand<Unit, Unit> LoginCommand { get; }
        public ReactiveCommand<Unit, Unit> VerifyMfaCommand { get; }

        public LoginViewModel()
        {
            // PSEUDO CODE: Initialize commands

            LoginCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                // PSEUDO CODE:
                // 1. Validate input (username, password not empty)
                // 2. Set IsLoading = true
                // 3. Call API: POST /api/auth/login
                // 4. Handle response:
                //    - If MFA required: Show MFA input, set MfaRequired = true
                //    - If success: Store JWT token, navigate to MainWindow
                //    - If error: Show ErrorMessage
                // 5. Set IsLoading = false

                IsLoading = true;
                ErrorMessage = string.Empty;

                try
                {
                    // var response = await _authService.LoginAsync(Username, Password);
                    // if (response.MfaRequired)
                    // {
                    //     MfaRequired = true;
                    // }
                    // else
                    // {
                    //     // Store tokens
                    //     _tokenStorage.SaveTokens(response.AccessToken, response.RefreshToken);
                    //     // Navigate to MainWindow
                    //     NavigateToMainWindow();
                    // }
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Login failed: " + ex.Message;
                }
                finally
                {
                    IsLoading = false;
                }
            });

            VerifyMfaCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                // PSEUDO CODE:
                // 1. Call API: POST /api/auth/verify-mfa
                // 2. If success: Store tokens, navigate to MainWindow
                // 3. If error: Show error message

                IsLoading = true;
                ErrorMessage = string.Empty;

                try
                {
                    // var response = await _authService.VerifyMfaAsync(sessionToken, MfaCode);
                    // _tokenStorage.SaveTokens(response.AccessToken, response.RefreshToken);
                    // NavigateToMainWindow();
                }
                catch (Exception ex)
                {
                    ErrorMessage = "MFA verification failed: " + ex.Message;
                }
                finally
                {
                    IsLoading = false;
                }
            });
        }
    }
}
