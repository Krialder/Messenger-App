namespace MessengerCommon.Constants
{
    /// <summary>
    /// Cryptographic constants used across the application
    /// </summary>
    public static class CryptoConstants
    {
        // Layer 1: ChaCha20-Poly1305
        public const int CHACHA20_KEY_SIZE = 32; // 256 bits
        public const int CHACHA20_NONCE_SIZE = 12; // 96 bits
        public const int POLY1305_TAG_SIZE = 16; // 128 bits

        // Layer 2: AES-256-GCM
        public const int AES_KEY_SIZE = 32; // 256 bits
        public const int AES_NONCE_SIZE = 12; // 96 bits
        public const int AES_TAG_SIZE = 16; // 128 bits

        // X25519 Key Exchange
        public const int X25519_KEY_SIZE = 32; // 256 bits

        // Argon2id Parameters
        public const int ARGON2_MEMORY_SIZE = 65536; // 64 MB
        public const int ARGON2_ITERATIONS = 3;
        public const int ARGON2_PARALLELISM = 4;
        public const int ARGON2_OUTPUT_SIZE = 32; // 256 bits

        // Salt sizes
        public const int SALT_SIZE = 32; // 256 bits
        
        // Performance targets (milliseconds)
        public const int LAYER1_ENCRYPTION_TARGET_MS = 100;
        public const int LAYER2_ENCRYPTION_TARGET_MS = 10;
        public const int LAYER3_ENCRYPTION_TARGET_MS = 10;
        public const int MASTER_KEY_DERIVATION_TARGET_MS = 200;
    }

    /// <summary>
    /// API endpoint constants
    /// </summary>
    public static class ApiEndpoints
    {
        // Auth Service
        public const string AUTH_BASE = "/api/auth";
        public const string AUTH_LOGIN = "/api/auth/login";
        public const string AUTH_REGISTER = "/api/auth/register";
        public const string AUTH_LOGOUT = "/api/auth/logout";

        // MFA
        public const string MFA_BASE = "/api/mfa";
        public const string MFA_SETUP = "/api/mfa/setup";
        public const string MFA_VERIFY = "/api/mfa/verify";
        public const string MFA_DISABLE = "/api/mfa/disable";

        // Messages
        public const string MESSAGES_BASE = "/api/messages";
        public const string MESSAGES_SEND = "/api/messages";
        public const string MESSAGES_GET = "/api/messages/{contactId}";
        
        // Users
        public const string USERS_BASE = "/api/users";
        public const string USERS_PROFILE = "/api/users/{userId}";
        public const string USERS_CONTACTS = "/api/users/contacts";

        // Keys
        public const string KEYS_BASE = "/api/keys";
        public const string KEYS_PUBLIC = "/api/keys/public/{userId}";
        public const string KEYS_ROTATE = "/api/keys/rotate";

        // Files
        public const string FILES_BASE = "/api/files";
        public const string FILES_UPLOAD = "/api/files/upload";
        public const string FILES_DOWNLOAD = "/api/files/{fileId}";
    }

    /// <summary>
    /// Configuration constants
    /// </summary>
    public static class ConfigConstants
    {
        // JWT
        public const int JWT_EXPIRATION_MINUTES = 60;
        public const int REFRESH_TOKEN_EXPIRATION_DAYS = 7;

        // Pagination
        public const int DEFAULT_PAGE_SIZE = 50;
        public const int MAX_PAGE_SIZE = 100;

        // Rate Limiting
        public const int MAX_LOGIN_ATTEMPTS = 5;
        public const int LOGIN_LOCKOUT_MINUTES = 15;

        // File Upload
        public const long MAX_FILE_SIZE_BYTES = 104857600; // 100 MB
        
        // Theme
        public const string DEFAULT_THEME = "DarkMode";
    }

    /// <summary>
    /// SignalR event names
    /// </summary>
    public static class SignalREvents
    {
        public const string MESSAGE_RECEIVED = "MessageReceived";
        public const string MESSAGE_READ = "MessageRead";
        public const string TYPING_STARTED = "TypingStarted";
        public const string TYPING_STOPPED = "TypingStopped";
        public const string USER_ONLINE = "UserOnline";
        public const string USER_OFFLINE = "UserOffline";
    }
}
