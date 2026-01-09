-- Database Initialization Script für Secure Messenger
-- Microservice-Architektur mit separaten Datenbanken

-- =============================================================================
-- MESSENGER_AUTH DATABASE
-- =============================================================================
CREATE DATABASE messenger_auth;
\c messenger_auth;

-- UUID Extension aktivieren
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Users Table
CREATE TABLE IF NOT EXISTS users(
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    username VARCHAR(50) UNIQUE NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash TEXT NOT NULL,
    master_key_salt BYTEA NOT NULL,
    mfa_enabled BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login_at TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    account_status VARCHAR(20) DEFAULT 'active',
    email_verified BOOLEAN DEFAULT FALSE,
    deletion_requested_at TIMESTAMP,
    deletion_scheduled_at TIMESTAMP,
    CONSTRAINT chk_username_length CHECK (char_length(username) >= 3),
    CONSTRAINT chk_account_status CHECK (account_status IN ('active', 'suspended', 'deleted'))
);

-- MFA Methods Table
CREATE TABLE IF NOT EXISTS mfa_methods (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    method_type VARCHAR(20) NOT NULL,
    totp_secret TEXT,
    yubikey_public_id TEXT,
    yubikey_credential_id BYTEA,
    fido2_credential_id BYTEA,
    fido2_public_key BYTEA,
    is_primary BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE,
    friendly_name VARCHAR(100),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_used_at TIMESTAMP,
    CONSTRAINT chk_method_type CHECK (method_type IN ('totp', 'yubikey', 'fido2'))
);

-- Recovery Codes Table
CREATE TABLE IF NOT EXISTS recovery_codes (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    code_hash TEXT NOT NULL,
    used BOOLEAN DEFAULT FALSE,
    used_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Refresh Tokens Table
CREATE TABLE IF NOT EXISTS refresh_tokens (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token TEXT NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    is_revoked BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    revoked_at TIMESTAMP
);

-- Indexes für messenger_auth
CREATE INDEX idx_users_username ON users(username);
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_active ON users(is_active, account_status);
CREATE INDEX idx_mfa_methods_user_id ON mfa_methods(user_id, is_active);
CREATE INDEX idx_recovery_codes_user_id ON recovery_codes(user_id, used);
CREATE INDEX idx_refresh_tokens_user_id ON refresh_tokens(user_id, is_revoked);
CREATE INDEX idx_refresh_tokens_token ON refresh_tokens(token) WHERE is_revoked = FALSE;

-- Kommentare für messenger_auth
COMMENT ON TABLE users IS 'Benutzer-Accounts mit MFA-Support und Account-Lifecycle';
COMMENT ON TABLE mfa_methods IS 'Multi-Factor Authentication Methoden (TOTP, YubiKey, FIDO2)';
COMMENT ON TABLE recovery_codes IS 'MFA Recovery Codes für Notfall-Zugriff';
COMMENT ON TABLE refresh_tokens IS 'JWT Refresh Tokens für Session Management';
COMMENT ON COLUMN users.master_key_salt IS 'Salt für Layer 2 Master Key Derivation (Argon2id)';

-- =============================================================================
-- MESSENGER_MESSAGES DATABASE (WITH GROUP CHAT SUPPORT)
-- =============================================================================
CREATE DATABASE messenger_messages;
\c messenger_messages;

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Conversations Table (NEW - Replaces direct messages)
CREATE TABLE IF NOT EXISTS conversations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    type INTEGER NOT NULL DEFAULT 0,  -- 0 = DirectMessage, 1 = Group
    name VARCHAR(100),  -- null for direct messages
    description VARCHAR(500),
    avatar_url VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by UUID NOT NULL,
    updated_at TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    CONSTRAINT chk_conversation_type CHECK (type IN (0, 1))
);

-- Conversation Members Table (NEW)
CREATE TABLE IF NOT EXISTS conversation_members (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    conversation_id UUID NOT NULL REFERENCES conversations(id) ON DELETE CASCADE,
    user_id UUID NOT NULL,
    role INTEGER NOT NULL DEFAULT 0,  -- 0 = Member, 1 = Admin, 2 = Owner
    joined_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    left_at TIMESTAMP,
    nickname VARCHAR(100),
    notifications_enabled BOOLEAN DEFAULT TRUE,
    last_read_at TIMESTAMP,
    CONSTRAINT chk_member_role CHECK (role IN (0, 1, 2))
);

-- Messages Table (UPDATED - Now references conversations)
CREATE TABLE IF NOT EXISTS messages (
    id UUID DEFAULT uuid_generate_v4(),
    conversation_id UUID NOT NULL REFERENCES conversations(id) ON DELETE CASCADE,
    sender_id UUID NOT NULL,
    encrypted_content BYTEA NOT NULL,
    nonce BYTEA NOT NULL,
    ephemeral_public_key BYTEA,
    encrypted_group_keys JSONB,  -- For group messages: encrypted keys per member
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    status INTEGER DEFAULT 0,  -- 0 = Sent, 1 = Delivered, 2 = Read
    is_deleted BOOLEAN DEFAULT FALSE,
    deleted_at TIMESTAMP,
    message_type INTEGER DEFAULT 0,  -- 0 = Text, 1 = Image, 2 = File, etc.
    reply_to_message_id UUID,
    PRIMARY KEY (id, created_at),
    CONSTRAINT chk_message_status CHECK (status IN (0, 1, 2)),
    CONSTRAINT chk_message_type CHECK (message_type IN (0, 1, 2, 3, 4, 5))
) PARTITION BY RANGE (created_at);

-- Partitions for 2025-2026
CREATE TABLE messages_2025_01 PARTITION OF messages
    FOR VALUES FROM ('2025-01-01') TO ('2025-02-01');
CREATE TABLE messages_2025_02 PARTITION OF messages
    FOR VALUES FROM ('2025-02-01') TO ('2025-03-01');
CREATE TABLE messages_2025_03 PARTITION OF messages
    FOR VALUES FROM ('2025-03-01') TO ('2025-04-01');
CREATE TABLE messages_2025_04 PARTITION OF messages
    FOR VALUES FROM ('2025-04-01') TO ('2025-05-01');
CREATE TABLE messages_2025_05 PARTITION OF messages
    FOR VALUES FROM ('2025-05-01') TO ('2025-06-01');
CREATE TABLE messages_2025_06 PARTITION OF messages
    FOR VALUES FROM ('2025-06-01') TO ('2025-07-01');
CREATE TABLE messages_2025_07 PARTITION OF messages
    FOR VALUES FROM ('2025-07-01') TO ('2025-08-01');
CREATE TABLE messages_2025_08 PARTITION OF messages
    FOR VALUES FROM ('2025-08-01') TO ('2025-09-01');
CREATE TABLE messages_2025_09 PARTITION OF messages
    FOR VALUES FROM ('2025-09-01') TO ('2025-10-01');
CREATE TABLE messages_2025_10 PARTITION OF messages
    FOR VALUES FROM ('2025-10-01') TO ('2025-11-01');
CREATE TABLE messages_2025_11 PARTITION OF messages
    FOR VALUES FROM ('2025-11-01') TO ('2025-12-01');
CREATE TABLE messages_2025_12 PARTITION OF messages
    FOR VALUES FROM ('2025-12-01') TO ('2026-01-01');
CREATE TABLE messages_2026_01 PARTITION OF messages
    FOR VALUES FROM ('2026-01-01') TO ('2026-02-01');

-- Indexes for Conversations
CREATE INDEX idx_conversations_created_by ON conversations(created_by);
CREATE INDEX idx_conversations_type ON conversations(type, is_active);
CREATE INDEX idx_conversations_created_at ON conversations(created_at);

-- Indexes for Conversation Members
CREATE INDEX idx_conversation_members_conversation_id ON conversation_members(conversation_id);
CREATE INDEX idx_conversation_members_user_id ON conversation_members(user_id);
CREATE INDEX idx_conversation_members_active ON conversation_members(user_id, left_at) WHERE left_at IS NULL;
CREATE INDEX idx_conversation_members_role ON conversation_members(conversation_id, role);

-- Indexes for Messages (on partitioned table)
CREATE INDEX idx_messages_conversation_id ON messages(conversation_id, created_at DESC);
CREATE INDEX idx_messages_sender_id ON messages(sender_id, created_at);
CREATE INDEX idx_messages_created_at ON messages(created_at);
CREATE INDEX idx_messages_active ON messages(conversation_id, is_deleted) WHERE is_deleted = FALSE;

-- Comments
COMMENT ON TABLE conversations IS 'Conversations (1-to-1 direct messages and group chats)';
COMMENT ON TABLE conversation_members IS 'Members of conversations with roles and permissions';
COMMENT ON TABLE messages IS 'End-to-End encrypted messages supporting both direct and group conversations';
COMMENT ON COLUMN messages.encrypted_group_keys IS 'JSONB: Encrypted group key for each member (group messages only)';
COMMENT ON COLUMN messages.ephemeral_public_key IS 'Ephemeral Public Key for Forward Secrecy (direct messages)';

-- =============================================================================
-- MESSENGER_KEYS DATABASE
-- =============================================================================
CREATE DATABASE messenger_keys;
\c messenger_keys;

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Public Keys Table
CREATE TABLE IF NOT EXISTS public_keys (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL,
    public_key BYTEA NOT NULL,
    key_type VARCHAR(20) DEFAULT 'X25519',
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    rotated_at TIMESTAMP,
    expires_at TIMESTAMP,
    CONSTRAINT chk_key_type CHECK (key_type IN ('X25519', 'Ed25519'))
);

CREATE INDEX idx_public_keys_user_id ON public_keys(user_id, is_active);
CREATE INDEX idx_public_keys_active ON public_keys(is_active, expires_at);

COMMENT ON TABLE public_keys IS 'Public Keys für Layer 1 E2E-Verschlüsselung';

-- =============================================================================
-- MESSENGER_USERS DATABASE
-- =============================================================================
CREATE DATABASE messenger_users;
\c messenger_users;

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Contacts Table
CREATE TABLE IF NOT EXISTS contacts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL,
    contact_user_id UUID NOT NULL,
    nickname VARCHAR(100),
    is_blocked BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    UNIQUE(user_id, contact_user_id),
    CONSTRAINT chk_no_self_contact CHECK (user_id != contact_user_id)
);

CREATE INDEX idx_contacts_user_id ON contacts(user_id);
CREATE INDEX idx_contacts_contact_user_id ON contacts(contact_user_id);

COMMENT ON TABLE contacts IS 'Kontaktliste mit Nickname-Support und Block-Funktion';

-- =============================================================================
-- MESSENGER_AUDIT DATABASE
-- =============================================================================
CREATE DATABASE messenger_audit;
\c messenger_audit;

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Audit Logs Table
CREATE TABLE IF NOT EXISTS audit_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID,
    event_type VARCHAR(50) NOT NULL,
    action VARCHAR(100) NOT NULL,
    details JSONB,
    ip_address INET,
    user_agent TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_event_type CHECK (event_type IN ('auth', 'message', 'user', 'key', 'file', 'admin'))
);

CREATE INDEX idx_audit_logs_user_id ON audit_logs(user_id, created_at);
CREATE INDEX idx_audit_logs_event_type ON audit_logs(event_type, created_at);
CREATE INDEX idx_audit_logs_created_at ON audit_logs(created_at);

COMMENT ON TABLE audit_logs IS 'Audit Trail für Security und Compliance';

-- =============================================================================
-- MESSENGER_FILES DATABASE (für FileTransferService)
-- =============================================================================
CREATE DATABASE messenger_files;
\c messenger_files;

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Encrypted Files Table
CREATE TABLE IF NOT EXISTS encrypted_files (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    sender_id UUID NOT NULL,
    recipient_id UUID NOT NULL,
    filename VARCHAR(255) NOT NULL,
    file_size BIGINT NOT NULL,
    encrypted_data BYTEA NOT NULL,
    encryption_key BYTEA NOT NULL,
    nonce BYTEA NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    downloaded_at TIMESTAMP,
    expires_at TIMESTAMP DEFAULT (CURRENT_TIMESTAMP + INTERVAL '7 days'),
    is_deleted BOOLEAN DEFAULT FALSE,
    CONSTRAINT chk_file_size CHECK (file_size <= 104857600)
);

CREATE INDEX idx_encrypted_files_sender ON encrypted_files(sender_id, created_at);
CREATE INDEX idx_encrypted_files_recipient ON encrypted_files(recipient_id, created_at);
CREATE INDEX idx_encrypted_files_expires ON encrypted_files(expires_at) WHERE is_deleted = FALSE;

COMMENT ON TABLE encrypted_files IS 'Verschlüsselte Datei-Uploads (max 100 MB, 7 Tage Retention)';

-- =============================================================================
-- MESSENGER_NOTIFICATIONS DATABASE (für NotificationService)
-- =============================================================================
CREATE DATABASE messenger_notifications;
\c messenger_notifications;

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Push Notification Tokens
CREATE TABLE IF NOT EXISTS notification_tokens (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL,
    device_id VARCHAR(255) NOT NULL,
    token TEXT NOT NULL,
    platform VARCHAR(20) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_used_at TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    UNIQUE(user_id, device_id),
    CONSTRAINT chk_platform CHECK (platform IN ('fcm', 'apns', 'web', 'signalr'))
);

-- Notification Preferences
CREATE TABLE IF NOT EXISTS notification_preferences (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL UNIQUE,
    message_notifications BOOLEAN DEFAULT TRUE,
    typing_indicators BOOLEAN DEFAULT TRUE,
    read_receipts BOOLEAN DEFAULT TRUE,
    online_status_visible BOOLEAN DEFAULT TRUE,
    sound_enabled BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP
);

-- Notification History (Optional - für Audit)
CREATE TABLE IF NOT EXISTS notification_history (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL,
    notification_type VARCHAR(50) NOT NULL,
    payload JSONB,
    sent_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    delivered_at TIMESTAMP,
    read_at TIMESTAMP,
    CONSTRAINT chk_notification_type CHECK (notification_type IN ('message', 'typing', 'read_receipt', 'online_status'))
);

CREATE INDEX idx_notification_tokens_user_id ON notification_tokens(user_id, is_active);
CREATE INDEX idx_notification_preferences_user_id ON notification_preferences(user_id);
CREATE INDEX idx_notification_history_user_id ON notification_history(user_id, sent_at);

COMMENT ON TABLE notification_tokens IS 'Push Notification Tokens für verschiedene Geräte und Plattformen';
COMMENT ON TABLE notification_preferences IS 'Benutzer-Einstellungen für Benachrichtigungen';
COMMENT ON TABLE notification_history IS 'Historie gesendeter Benachrichtigungen (Optional für Debugging)';
