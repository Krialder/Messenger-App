using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using MessengerClient.Services;
using MessengerClient.Data;
using MessengerContracts.DTOs;

namespace MessengerClient.ViewModels
{
    public class ContactsViewModel : ReactiveObject
    {
        private readonly IUserApiService _userApi;
        private readonly LocalStorageService _localStorage;

        private ObservableCollection<ContactViewModel> _contacts;
        private string _searchQuery = string.Empty;
        private bool _isLoading;
        private string? _jwtToken;

        public ObservableCollection<ContactViewModel> Contacts
        {
            get => _contacts;
            set => this.RaiseAndSetIfChanged(ref _contacts, value);
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                this.RaiseAndSetIfChanged(ref _searchQuery, value);
                _ = SearchUsersAsync();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public ReactiveCommand<Unit, Unit> RefreshContactsCommand { get; }
        public ReactiveCommand<ContactViewModel, Unit> AddContactCommand { get; }
        public ReactiveCommand<ContactViewModel, Unit> RemoveContactCommand { get; }

        public ContactsViewModel(
            IUserApiService userApi,
            LocalStorageService localStorage)
        {
            _userApi = userApi;
            _localStorage = localStorage;

            _contacts = new ObservableCollection<ContactViewModel>();

            RefreshContactsCommand = ReactiveCommand.CreateFromTask(LoadContactsAsync);
            AddContactCommand = ReactiveCommand.CreateFromTask<ContactViewModel>(AddContactAsync);
            RemoveContactCommand = ReactiveCommand.CreateFromTask<ContactViewModel>(RemoveContactAsync);

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            _jwtToken = await _localStorage.GetTokenAsync();
            await LoadContactsAsync();
        }

        private async Task LoadContactsAsync()
        {
            if (string.IsNullOrEmpty(_jwtToken)) return;

            try
            {
                IsLoading = true;
                List<MessengerClient.Services.ContactDto> contacts = await _userApi.GetContactsAsync($"Bearer {_jwtToken}");

                Contacts.Clear();
                foreach (var contact in contacts)
                {
                    await _localStorage.SaveContactAsync(new LocalContact
                    {
                        Id = contact.Id,
                        UserId = contact.UserId,
                        DisplayName = contact.DisplayName ?? string.Empty,
                        Email = contact.Email ?? string.Empty,
                        IsOnline = contact.IsOnline
                    });

                    Contacts.Add(new ContactViewModel
                    {
                        Id = contact.Id,
                        UserId = contact.UserId,
                        DisplayName = contact.DisplayName ?? string.Empty,
                        Email = contact.Email ?? string.Empty,
                        IsOnline = contact.IsOnline
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading contacts: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchUsersAsync()
        {
            if (string.IsNullOrEmpty(_jwtToken) || string.IsNullOrWhiteSpace(SearchQuery)) return;

            try
            {
                List<UserDto> users = await _userApi.SearchUsersAsync(SearchQuery, $"Bearer {_jwtToken}");
                // TODO: Handle search results
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching users: {ex.Message}");
            }
        }

        private async Task AddContactAsync(ContactViewModel contact)
        {
            if (string.IsNullOrEmpty(_jwtToken)) return;

            try
            {
                var request = new MessengerClient.Services.AddContactRequest(contact.UserId);

                var newContact = await _userApi.AddContactAsync(request, $"Bearer {_jwtToken}");
                await LoadContactsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding contact: {ex.Message}");
            }
        }

        private async Task RemoveContactAsync(ContactViewModel contact)
        {
            if (string.IsNullOrEmpty(_jwtToken)) return;

            try
            {
                await _userApi.DeleteContactAsync(contact.Id, $"Bearer {_jwtToken}");
                Contacts.Remove(contact);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing contact: {ex.Message}");
            }
        }
    }

    public class ContactViewModel : ReactiveObject
    {
        private bool _isOnline;

        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public bool IsOnline
        {
            get => _isOnline;
            set => this.RaiseAndSetIfChanged(ref _isOnline, value);
        }
    }
}
