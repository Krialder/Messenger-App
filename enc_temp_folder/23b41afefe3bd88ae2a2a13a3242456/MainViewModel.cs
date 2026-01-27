using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;

namespace MessengerClient.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        private ReactiveObject _currentViewModel;
        private bool _isChatViewActive = true;
        private bool _isContactsViewActive = false;
        private bool _isSettingsViewActive = false;

        public ChatViewModel ChatViewModel { get; }
        public ContactsViewModel ContactsViewModel { get; }
        public SettingsViewModel SettingsViewModel { get; }

        public ReactiveCommand<Unit, Unit> NavigateToChatCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> NavigateToContactsCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> NavigateToSettingsCommand { get; private set; }

        public bool IsChatViewActive
        {
            get => _isChatViewActive;
            private set => this.RaiseAndSetIfChanged(ref _isChatViewActive, value);
        }

        public bool IsContactsViewActive
        {
            get => _isContactsViewActive;
            private set => this.RaiseAndSetIfChanged(ref _isContactsViewActive, value);
        }

        public bool IsSettingsViewActive
        {
            get => _isSettingsViewActive;
            private set => this.RaiseAndSetIfChanged(ref _isSettingsViewActive, value);
        }

        public MainViewModel(
            ChatViewModel chatViewModel,
            ContactsViewModel contactsViewModel,
            SettingsViewModel settingsViewModel)
        {
            ChatViewModel = chatViewModel;
            ContactsViewModel = contactsViewModel;
            SettingsViewModel = settingsViewModel;

            _currentViewModel = ChatViewModel;

            NavigateToChatCommand = ReactiveCommand.Create(NavigateToChat);
            NavigateToContactsCommand = ReactiveCommand.Create(NavigateToContacts);
            NavigateToSettingsCommand = ReactiveCommand.Create(NavigateToSettings);
        }

        public void NavigateToChat()
        {
            IsChatViewActive = true;
            IsContactsViewActive = false;
            IsSettingsViewActive = false;
        }

        public void NavigateToContacts()
        {
            IsChatViewActive = false;
            IsContactsViewActive = true;
            IsSettingsViewActive = false;
        }

        public void NavigateToSettings()
        {
            IsChatViewActive = false;
            IsContactsViewActive = false;
            IsSettingsViewActive = true;
        }
    }
}
