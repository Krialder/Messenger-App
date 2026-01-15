using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using MessengerClient.Services;
using MessengerClient.Data;
using MessengerContracts.DTOs;
using ContractsMessageDto = MessengerContracts.DTOs.MessageDto;
using ContractsConversationDto = MessengerContracts.DTOs.ConversationDto;

namespace MessengerClient.ViewModels
{
    public class ChatViewModel : ReactiveObject, IDisposable
    {
        private readonly IMessageApiService _messageApi;
        private readonly ICryptoApiService _cryptoApi;
        private readonly SignalRService _signalR;
        private readonly LocalStorageService _localStorage;
        private readonly LocalCryptoService _crypto;

        private ObservableCollection<MessageViewModel> _messages;
        private ObservableCollection<ConversationViewModel> _conversations;
        private ConversationViewModel? _selectedConversation;
        private string _messageText = string.Empty;
        private bool _isLoading;
        private string? _jwtToken;

        public ObservableCollection<MessageViewModel> Messages
        {
            get => _messages;
            set => this.RaiseAndSetIfChanged(ref _messages, value);
        }

        public ObservableCollection<ConversationViewModel> Conversations
        {
            get => _conversations;
            set => this.RaiseAndSetIfChanged(ref _conversations, value);
        }

        public ConversationViewModel? SelectedConversation
        {
            get => _selectedConversation;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedConversation, value);
                if (value != null)
                {
                    _ = LoadMessagesAsync(value.Id);
                }
            }
        }

        public string MessageText
        {
            get => _messageText;
            set => this.RaiseAndSetIfChanged(ref _messageText, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public ReactiveCommand<Unit, Unit> SendMessageCommand { get; }
        public ReactiveCommand<Unit, Unit> SendFileCommand { get; }
        public ReactiveCommand<Unit, Unit> RefreshConversationsCommand { get; }

        public ChatViewModel(
            IMessageApiService messageApi,
            ICryptoApiService cryptoApi,
            SignalRService signalR,
            LocalStorageService localStorage,
            LocalCryptoService crypto)
        {
            _messageApi = messageApi;
            _cryptoApi = cryptoApi;
            _signalR = signalR;
            _localStorage = localStorage;
            _crypto = crypto;

            _messages = new ObservableCollection<MessageViewModel>();
            _conversations = new ObservableCollection<ConversationViewModel>();

            IObservable<bool> canSend = this.WhenAnyValue(
                x => x.MessageText,
                x => x.SelectedConversation,
                x => x.IsLoading,
                (text, conversation, loading) =>
                    !string.IsNullOrWhiteSpace(text) &&
                    conversation != null &&
                    !loading);

            SendMessageCommand = ReactiveCommand.CreateFromTask(SendMessageAsync, canSend);
            SendFileCommand = ReactiveCommand.CreateFromTask(SendFileAsync);
            RefreshConversationsCommand = ReactiveCommand.CreateFromTask(LoadConversationsAsync);

            _signalR.OnMessageReceived += HandleNewMessageAsync;

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            _jwtToken = await _localStorage.GetTokenAsync();
            await LoadConversationsAsync();
        }

        private async Task LoadConversationsAsync()
        {
            if (string.IsNullOrEmpty(_jwtToken)) return;

            try
            {
                IsLoading = true;
                List<ContractsConversationDto> conversations = await _messageApi.GetConversationsAsync($"Bearer {_jwtToken}");

                Conversations.Clear();
                foreach (ContractsConversationDto conv in conversations)
                {
                    await _localStorage.SaveConversationAsync(new LocalConversation
                    {
                        Id = conv.Id,
                        Type = conv.Type.ToString(),
                        Name = conv.Name,
                        CreatedAt = conv.CreatedAt,
                        LastMessageAt = conv.LastMessage?.Timestamp // Changed from LastMessageAt
                    });

                    Conversations.Add(new ConversationViewModel
                    {
                        Id = conv.Id,
                        Name = conv.Name ?? "Unknown",
                        LastMessage = conv.LastMessage?.EncryptedContent ?? string.Empty, // Changed from LastMessagePreview
                        LastMessageTime = conv.LastMessage?.Timestamp ?? conv.CreatedAt, // Changed from LastMessageAt
                        UnreadCount = conv.UnreadCount
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading conversations: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadMessagesAsync(Guid conversationId)
        {
            if (string.IsNullOrEmpty(_jwtToken)) return;

            try
            {
                IsLoading = true;
                Messages.Clear();

                List<ContractsMessageDto> messages = await _messageApi.GetMessagesAsync(conversationId, $"Bearer {_jwtToken}");

                foreach (ContractsMessageDto msg in messages)
                {
                    string decryptedContent = await DecryptMessageAsync(msg);

                    Messages.Add(new MessageViewModel
                    {
                        Id = msg.Id,
                        Content = decryptedContent,
                        SenderId = msg.SenderId,
                        Timestamp = msg.Timestamp, // Changed from SentAt to Timestamp
                        IsSent = msg.SenderId == (await GetCurrentUserIdAsync()),
                        IsDelivered = msg.Status == MessageStatus.Delivered || msg.Status == MessageStatus.Read,
                        IsRead = msg.Status == MessageStatus.Read
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading messages: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SendMessageAsync()
        {
            if (SelectedConversation == null || string.IsNullOrEmpty(_jwtToken)) return;

            string plaintext = string.Empty; // Declare at the start
            try
            {
                plaintext = MessageText;
                MessageText = string.Empty;

                // Layer 2: Local storage encryption
                string encryptedLocal = await _crypto.EncryptLocalDataAsync(plaintext);

                // Get recipient public key
                LocalContact? recipient = await GetRecipientForConversationAsync(SelectedConversation.Id);
                if (recipient?.PublicKey == null)
                {
                    Console.WriteLine("Recipient public key not found");
                    return;
                }

                // Layer 1: Transport encryption
                var encryptRequest = new EncryptRequest(encryptedLocal, Convert.ToBase64String(recipient.PublicKey));
                EncryptResponse encrypted = await _cryptoApi.EncryptAsync(encryptRequest, $"Bearer {_jwtToken}");

                // Send message
                var sendRequest = new MessageServiceSendMessageRequest(SelectedConversation.Id, encrypted.EncryptedData, null);
                ContractsMessageDto response = await _messageApi.SendMessageAsync(sendRequest, $"Bearer {_jwtToken}");

                // Add to UI
                Guid currentUserId = await GetCurrentUserIdAsync();
                Messages.Add(new MessageViewModel
                {
                    Id = response.Id,
                    Content = plaintext,
                    SenderId = currentUserId,
                    Timestamp = DateTime.UtcNow,
                    IsSent = true,
                    IsDelivered = false,
                    IsRead = false
                });

                // Save locally
                await _localStorage.SaveMessageAsync(new LocalMessage
                {
                    Id = response.Id,
                    ConversationId = SelectedConversation.Id,
                    SenderId = currentUserId,
                    Content = plaintext,
                    EncryptedContent = encryptedLocal,
                    Timestamp = DateTime.UtcNow,
                    IsSent = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                MessageText = plaintext; // Restore message on error
            }
        }

        private async Task SendFileAsync()
        {
            await Task.CompletedTask;
        }

        private async Task HandleNewMessageAsync(ContractsMessageDto message)
        {
            try
            {
                string decryptedContent = await DecryptMessageAsync(message);

                Guid currentUserId = await GetCurrentUserIdAsync();
                Messages.Add(new MessageViewModel
                {
                    Id = message.Id,
                    Content = decryptedContent,
                    SenderId = message.SenderId,
                    Timestamp = message.Timestamp, // Changed from SentAt
                    IsSent = message.SenderId == currentUserId,
                    IsDelivered = true,
                    IsRead = false
                });

                await _localStorage.SaveMessageAsync(new LocalMessage
                {
                    Id = message.Id,
                    ConversationId = message.ConversationId,
                    SenderId = message.SenderId,
                    Content = decryptedContent,
                    Timestamp = message.Timestamp, // Changed from SentAt
                    IsSent = false,
                    IsDelivered = true
                });

                ConversationViewModel? conversation = Conversations.FirstOrDefault(c => c.Id == message.ConversationId);
                if (conversation != null)
                {
                    conversation.LastMessage = decryptedContent;
                    conversation.LastMessageTime = message.Timestamp; // Changed from SentAt
                    conversation.UnreadCount++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling new message: {ex.Message}");
            }
        }

        private async Task<string> DecryptMessageAsync(ContractsMessageDto message)
        {
            if (string.IsNullOrEmpty(_jwtToken)) return "[Encrypted]";

            try
            {
                // Layer 1: Transport decryption
                LocalKeyPair? keyPair = await _localStorage.GetActiveKeyPairAsync();
                if (keyPair == null) return "[No key]" ;

                var decryptRequest = new DecryptRequest(message.EncryptedContent, string.Empty, Convert.ToBase64String(keyPair.PrivateKey)); // Changed from Content
                DecryptResponse response = await _cryptoApi.DecryptAsync(decryptRequest, $"Bearer {_jwtToken}");

                // Layer 2: Local storage decryption
                string decryptedLocal = await _crypto.DecryptLocalDataAsync(response.Plaintext);
                return decryptedLocal;
            }
            catch
            {
                return "[Decryption failed]";
            }
        }

        private async Task<LocalContact?> GetRecipientForConversationAsync(Guid conversationId)
        {
            List<LocalContact> contacts = await _localStorage.GetContactsAsync();
            return contacts.FirstOrDefault(); // TODO: Proper recipient lookup
        }

        private async Task<Guid> GetCurrentUserIdAsync()
        {
            LocalUserProfile? profile = await _localStorage.GetUserProfileAsync();
            return profile?.UserId ?? Guid.Empty;
        }

        public void Dispose()
        {
            // Unsubscribe from SignalR events to prevent memory leaks
            _signalR.OnMessageReceived -= HandleNewMessageAsync;
        }
    }

    public class MessageViewModel : ReactiveObject
    {
        private bool _isDelivered;
        private bool _isRead;

        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public Guid SenderId { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsSent { get; set; }

        public bool IsDelivered
        {
            get => _isDelivered;
            set => this.RaiseAndSetIfChanged(ref _isDelivered, value);
        }

        public bool IsRead
        {
            get => _isRead;
            set => this.RaiseAndSetIfChanged(ref _isRead, value);
        }
    }

    public class ConversationViewModel : ReactiveObject
    {
        private string _lastMessage = string.Empty;
        private DateTime _lastMessageTime;
        private int _unreadCount;

        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public string LastMessage
        {
            get => _lastMessage;
            set => this.RaiseAndSetIfChanged(ref _lastMessage, value);
        }

        public DateTime LastMessageTime
        {
            get => _lastMessageTime;
            set => this.RaiseAndSetIfChanged(ref _lastMessageTime, value);
        }

        public int UnreadCount
        {
            get => _unreadCount;
            set => this.RaiseAndSetIfChanged(ref _unreadCount, value);
        }
    }
}
