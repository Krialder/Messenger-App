using ReactiveUI;
using System;
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
                List<ContactDto> contacts = await _userApi.GetContactsAsync($"Bearer {_jwtToken}");

                Contacts.Clear();
                foreach (ContactDto contact in contacts)
                {
                    await _localStorage.SaveContactAsync(new LocalContact
                    {
                        Id = contact.Id,
                        UserId = contact.UserId,
                        DisplayName = contact.DisplayName,
                        Email = contact.Email,
                        IsOnline = false
                    });

                    Contacts.Add(new ContactViewModel
                    {
                        Id = contact.Id,
                        UserId = contact.UserId,
                        DisplayName = contact.DisplayName,
                        Email = contact.Email ?? string.Empty,
                        IsOnline = false
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
                List<UserDto> users = await _userApi.SearchUsersAsync($"Bearer {_jwtToken}", SearchQuery);
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
                AddContactRequest request = new AddContactRequest(contact.UserId);

                ContactDto newContact = await _userApi.AddContactAsync($"Bearer {_jwtToken}", request);
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
                await _userApi.DeleteContactAsync($"Bearer {_jwtToken}", contact.Id);
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
