using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using System.Linq;
using MessengerClient.Services;
using MessengerClient.Data;
using MessengerContracts.DTOs;

namespace MessengerClient.ViewModels
{
    public class ChatViewModel : ReactiveObject
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
                List<ConversationDto> conversations = await _messageApi.GetConversationsAsync($"Bearer {_jwtToken}");

                Conversations.Clear();
                foreach (ConversationDto conv in conversations)
                {
                    await _localStorage.SaveConversationAsync(new LocalConversation
                    {
                        Id = conv.Id,
                        Type = conv.Type.ToString(),
                        Name = conv.Name,
                        CreatedAt = conv.CreatedAt,
                        LastMessageAt = conv.LastMessageAt
                    });

                    Conversations.Add(new ConversationViewModel
                    {
                        Id = conv.Id,
                        Name = conv.Name ?? "Unknown",
                        LastMessage = conv.LastMessagePreview ?? string.Empty,
                        LastMessageTime = conv.LastMessageAt ?? conv.CreatedAt,
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

                List<MessageDto> messages = await _messageApi.GetMessagesAsync($"Bearer {_jwtToken}", conversationId);

                foreach (MessageDto msg in messages)
                {
                    string decryptedContent = await DecryptMessageAsync(msg);

                    Messages.Add(new MessageViewModel
                    {
                        Id = msg.Id,
                        Content = decryptedContent,
                        SenderId = msg.SenderId,
                        Timestamp = msg.SentAt,
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

            try
            {
                string plaintext = MessageText;
                MessageText = string.Empty;

                string encryptedLocal = await _crypto.EncryptLocalDataAsync(plaintext);

                LocalContact? recipient = await GetRecipientForConversationAsync(SelectedConversation.Id);
                if (recipient?.PublicKey == null)
                {
                    Console.WriteLine("Recipient public key not found");
                    return;
                }

                EncryptedMessageDto encrypted = await _cryptoApi.EncryptTransportAsync(
                    $"Bearer {_jwtToken}",
                    new EncryptRequest
                    {
                        Plaintext = encryptedLocal,
                        RecipientPublicKey = recipient.PublicKey
                    }
                );

                SendMessageRequest request = new SendMessageRequest
                {
                    ConversationId = SelectedConversation.Id,
                    Content = Convert.ToBase64String(encrypted.Ciphertext),
                    Nonce = encrypted.Nonce,
                    Tag = encrypted.Tag,
                    EphemeralPublicKey = encrypted.EphemeralPublicKey
                };

                MessageResponse response = await _messageApi.SendMessageAsync($"Bearer {_jwtToken}", request);

                Guid currentUserId = await GetCurrentUserIdAsync();
                Messages.Add(new MessageViewModel
                {
                    Id = response.MessageId,
                    Content = plaintext,
                    SenderId = currentUserId,
                    Timestamp = DateTime.UtcNow,
                    IsSent = true,
                    IsDelivered = false,
                    IsRead = false
                });

                await _localStorage.SaveMessageAsync(new LocalMessage
                {
                    Id = response.MessageId,
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
                MessageText = MessageText;
            }
        }

        private async Task SendFileAsync()
        {
            await Task.CompletedTask;
        }

        private async Task HandleNewMessageAsync(MessageDto message)
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
                    Timestamp = message.SentAt,
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
                    Timestamp = message.SentAt,
                    IsSent = false,
                    IsDelivered = true
                });

                ConversationViewModel? conversation = Conversations.FirstOrDefault(c => c.Id == message.ConversationId);
                if (conversation != null)
                {
                    conversation.LastMessage = decryptedContent;
                    conversation.LastMessageTime = message.SentAt;
                    conversation.UnreadCount++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling new message: {ex.Message}");
            }
        }

        private async Task<string> DecryptMessageAsync(MessageDto message)
        {
            if (string.IsNullOrEmpty(_jwtToken)) return "[Encrypted]";

            try
            {
                LocalKeyPair? keyPair = await _localStorage.GetActiveKeyPairAsync();
                if (keyPair == null) return "[No key]";

                DecryptResponse response = await _cryptoApi.DecryptTransportAsync(
                    $"Bearer {_jwtToken}",
                    new DecryptRequest
                    {
                        Ciphertext = Convert.FromBase64String(message.Content),
                        Nonce = message.Nonce,
                        Tag = message.Tag,
                        EphemeralPublicKey = message.EphemeralPublicKey,
                        PrivateKey = keyPair.PrivateKey
                    }
                );

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
            return contacts.FirstOrDefault();
        }

        private async Task<Guid> GetCurrentUserIdAsync()
        {
            LocalUserProfile? profile = await _localStorage.GetUserProfileAsync();
            return profile?.UserId ?? Guid.Empty;
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
