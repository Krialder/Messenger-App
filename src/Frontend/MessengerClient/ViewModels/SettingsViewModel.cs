using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using System.Collections.Generic;
using MessengerClient.Services;
using MessengerContracts.DTOs;

namespace MessengerClient.ViewModels
{
    public class SettingsViewModel : ReactiveObject
    {
        private readonly IAuthApiService _authApi;
        private readonly LocalStorageService _localStorage;
        private readonly SignalRService _signalR;

        private string _displayName = string.Empty;
        private bool _mfaEnabled;
        private bool _darkModeEnabled = true;
        private string _qrCodeUrl = string.Empty;
        private string _secretKey = string.Empty;
        private List<string> _backupCodes = new List<string>();
        private bool _showMfaSetup;

        public string DisplayName
        {
            get => _displayName;
            set => this.RaiseAndSetIfChanged(ref _displayName, value);
        }

        public bool MfaEnabled
        {
            get => _mfaEnabled;
            set => this.RaiseAndSetIfChanged(ref _mfaEnabled, value);
        }

        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set => this.RaiseAndSetIfChanged(ref _darkModeEnabled, value);
        }

        public string QrCodeUrl
        {
            get => _qrCodeUrl;
            set => this.RaiseAndSetIfChanged(ref _qrCodeUrl, value);
        }

        public string SecretKey
        {
            get => _secretKey;
            set => this.RaiseAndSetIfChanged(ref _secretKey, value);
        }

        public List<string> BackupCodes
        {
            get => _backupCodes;
            set => this.RaiseAndSetIfChanged(ref _backupCodes, value);
        }

        public bool ShowMfaSetup
        {
            get => _showMfaSetup;
            set => this.RaiseAndSetIfChanged(ref _showMfaSetup, value);
        }

        public ReactiveCommand<Unit, Unit> EnableMfaCommand { get; }
        public ReactiveCommand<Unit, Unit> LogoutCommand { get; }

        public event EventHandler? LoggedOut;

        public SettingsViewModel(
            IAuthApiService authApi,
            LocalStorageService localStorage,
            SignalRService signalR)
        {
            _authApi = authApi;
            _localStorage = localStorage;
            _signalR = signalR;

            EnableMfaCommand = ReactiveCommand.CreateFromTask(EnableMfaAsync);
            LogoutCommand = ReactiveCommand.CreateFromTask(LogoutAsync);

            _ = LoadUserProfileAsync();
        }

        private async Task LoadUserProfileAsync()
        {
            Data.LocalUserProfile? profile = await _localStorage.GetUserProfileAsync();
            if (profile != null)
            {
                DisplayName = profile.DisplayName;
                MfaEnabled = profile.MfaEnabled;
            }
        }

        private async Task EnableMfaAsync()
        {
            string? token = await _localStorage.GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return;

            try
            {
                EnableTotpResponse response = await _authApi.SetupMfaAsync($"Bearer {token}");
                QrCodeUrl = response.QrCodeUrl;
                SecretKey = response.Secret;
                BackupCodes = response.BackupCodes;
                ShowMfaSetup = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enabling MFA: {ex.Message}");
            }
        }

        private async Task LogoutAsync()
        {
            await _signalR.DisconnectAsync();
            await _localStorage.ClearTokenAsync();
            LoggedOut?.Invoke(this, EventArgs.Empty);
        }
    }
}
