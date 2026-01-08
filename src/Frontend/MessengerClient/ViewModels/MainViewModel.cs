using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;

namespace MessengerClient.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        // PSEUDO CODE: Main Chat Window ViewModel

        private ObservableCollection<ContactViewModel> _contacts = new();
        private ObservableCollection<MessageViewModel> _messages = new();
        private ContactViewModel? _selectedContact;
        private string _messageInput = string.Empty;
        private bool _isTyping;
        private bool _privacyModeEnabled;

        public ObservableCollection<ContactViewModel> Contacts
        {
            get => _contacts;
            set => this.RaiseAndSetIfChanged(ref _contacts, value);
        }

        public ObservableCollection<MessageViewModel> Messages
        {
            get => _messages;
            set => this.RaiseAndSetIfChanged(ref _messages, value);
        }

        public ContactViewModel? SelectedContact
        {
            get => _selectedContact;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedContact, value);
                if (value != null)
                {
                    LoadMessages(value.UserId);
                }
            }
        }

        public string MessageInput
        {
            get => _messageInput;
            set
            {
                this.RaiseAndSetIfChanged(ref _messageInput, value);
                SendTypingIndicator();
            }
        }

        public bool PrivacyModeEnabled
        {
            get => _privacyModeEnabled;
            set => this.RaiseAndSetIfChanged(ref _privacyModeEnabled, value);
        }

        public ReactiveCommand<Unit, Unit> SendMessageCommand { get; }
        public ReactiveCommand<Unit, Unit> TogglePrivacyModeCommand { get; }

        public MainViewModel()
        {
            // PSEUDO CODE: Initialize commands and SignalR

            SendMessageCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                // PSEUDO CODE:
                // 1. Get message text from MessageInput
                // 2. Get recipient public key from SelectedContact
                // 3. Encrypt message with Layer 1 (ChaCha20-Poly1305)
                //    - plaintext = MessageInput
                //    - encrypted = Layer1.Encrypt(plaintext, recipientPublicKey, myPrivateKey)
                // 4. Send to API: POST /api/messages/send
                // 5. Add to local Messages collection
                // 6. Clear MessageInput
                // 7. Store in local cache (encrypted with Layer 2)

                if (string.IsNullOrWhiteSpace(MessageInput) || SelectedContact == null)
                    return;

                // var encrypted = await _cryptoService.EncryptMessageAsync(MessageInput, SelectedContact.PublicKey);
                // await _messageService.SendMessageAsync(SelectedContact.UserId, encrypted);
                
                Messages.Add(new MessageViewModel
                {
                    Content = MessageInput,
                    IsSentByMe = true,
                    Timestamp = DateTime.Now
                });

                MessageInput = string.Empty;
            });

            TogglePrivacyModeCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                // PSEUDO CODE:
                // 1. If enabling: Prompt for PIN
                // 2. Derive device key (Layer 3)
                // 3. Obfuscate all displayed messages
                // 4. If disabling: Clear device key, show plain messages

                if (!PrivacyModeEnabled)
                {
                    // var pin = await ShowPinDialog();
                    // _displayEncryption.EnablePrivacyMode(pin);
                    PrivacyModeEnabled = true;
                    ObfuscateMessages();
                }
                else
                {
                    // _displayEncryption.DisablePrivacyMode();
                    PrivacyModeEnabled = false;
                    DeobfuscateMessages();
                }
            });

            // Initialize SignalR connection
            InitializeSignalR();
        }

        private async void InitializeSignalR()
        {
            // PSEUDO CODE:
            // 1. Connect to SignalR hub: wss://api.messenger.com/hubs/notifications
            // 2. Register event handlers:
            //    - OnNewMessage: Add to Messages collection, decrypt
            //    - OnTypingIndicator: Show typing animation
            //    - OnUserOnline: Update contact status
            // 3. Reconnect on disconnect

            // _hubConnection.On<MessageDto>("NewMessage", async (message) =>
            // {
            //     var decrypted = await _cryptoService.DecryptMessageAsync(message);
            //     Messages.Add(new MessageViewModel { Content = decrypted, IsSentByMe = false });
            // });
        }

        private async void LoadMessages(Guid contactId)
        {
            // PSEUDO CODE:
            // 1. Fetch message history from API
            // 2. Decrypt each message (Layer 1)
            // 3. If Privacy Mode enabled: Obfuscate (Layer 3)
            // 4. Display in Messages collection

            Messages.Clear();
            // var history = await _messageService.GetMessageHistoryAsync(contactId);
            // foreach (var msg in history)
            // {
            //     var decrypted = await _cryptoService.DecryptMessageAsync(msg);
            //     Messages.Add(new MessageViewModel { Content = decrypted, ... });
            // }
        }

        private void SendTypingIndicator()
        {
            // PSEUDO CODE:
            // 1. Throttle: Only send once per 3 seconds
            // 2. Emit SignalR event to recipient
            // 3. Auto-stop after 5 seconds of no input

            if (SelectedContact != null && !_isTyping)
            {
                _isTyping = true;
                // _hubConnection.InvokeAsync("SendTypingIndicator", SelectedContact.UserId);
                
                // Auto-stop after 5 seconds
                // Task.Delay(5000).ContinueWith(_ => _isTyping = false);
            }
        }

        private void ObfuscateMessages()
        {
            // PSEUDO CODE: Apply Layer 3 obfuscation
            foreach (var msg in Messages)
            {
                // msg.Content = _displayEncryption.ObfuscateMessage(msg.Content);
            }
        }

        private void DeobfuscateMessages()
        {
            // PSEUDO CODE: Remove Layer 3 obfuscation
            foreach (var msg in Messages)
            {
                // msg.Content = _displayEncryption.DeobfuscateMessage(msg.Content);
            }
        }
    }

    public class ContactViewModel
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public string LastMessage { get; set; } = string.Empty;
        public byte[]? PublicKey { get; set; }
    }

    public class MessageViewModel
    {
        public string Content { get; set; } = string.Empty;
        public bool IsSentByMe { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }
    }
}
