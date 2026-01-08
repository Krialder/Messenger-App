-- PSEUDO CODE: Database Initialization Script
-- Creates all required databases and schemas

-- Create databases
CREATE DATABASE messenger_auth;
CREATE DATABASE messenger_messages;
CREATE DATABASE messenger_keys;
CREATE DATABASE messenger_users;
CREATE DATABASE messenger_audit;

-- Connect to messenger_auth database
\c messenger_auth;

-- Users table
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(50) UNIQUE NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash TEXT NOT NULL,
    master_key_salt BYTEA NOT NULL,
    mfa_enabled BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT NOW(),
    last_login_at TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    account_status VARCHAR(20) DEFAULT 'active',
    email_verified BOOLEAN DEFAULT FALSE,
    CONSTRAINT chk_username_length CHECK (char_length(username) >= 3)
);

-- MFA Methods table
CREATE TABLE mfa_methods (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    method_type VARCHAR(20) NOT NULL,
    totp_secret TEXT,
    yubikey_public_id TEXT,
    yubikey_credential_id BYTEA,
    fido2_credential_id BYTEA,
    fido2_public_key BYTEA,
    is_primary BOOLEAN DEFAULT FALSE,
    friendly_name VARCHAR(100),
    created_at TIMESTAMP DEFAULT NOW(),
    last_used_at TIMESTAMP,
    CONSTRAINT chk_method_type CHECK (method_type IN ('totp', 'yubikey', 'fido2'))
);

-- Recovery Codes table
CREATE TABLE recovery_codes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    code_hash TEXT NOT NULL,
    used BOOLEAN DEFAULT FALSE,
    used_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW()
);

-- Refresh Tokens table
CREATE TABLE refresh_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token TEXT NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    is_revoked BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT NOW()
);

-- Indexes
CREATE INDEX idx_users_username ON users(username);
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_mfa_methods_user_id ON mfa_methods(user_id);
CREATE INDEX idx_recovery_codes_user_id ON recovery_codes(user_id);

-- Connect to messenger_messages database
\c messenger_messages;

-- Messages table (partitioned by date)
CREATE TABLE messages (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sender_id UUID NOT NULL,
    recipient_id UUID NOT NULL,
    encrypted_content BYTEA NOT NULL,
    nonce BYTEA NOT NULL,
    ephemeral_public_key BYTEA NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    read_at TIMESTAMP,
    deleted_at TIMESTAMP
) PARTITION BY RANGE (created_at);

-- Create partitions (monthly)
CREATE TABLE messages_2024_01 PARTITION OF messages
    FOR VALUES FROM ('2024-01-01') TO ('2024-02-01');

-- Indexes
CREATE INDEX idx_messages_sender ON messages(sender_id);
CREATE INDEX idx_messages_recipient ON messages(recipient_id);
CREATE INDEX idx_messages_created_at ON messages(created_at);

-- Connect to messenger_keys database
\c messenger_keys;

-- Public Keys table
CREATE TABLE public_keys (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    public_key BYTEA NOT NULL,
    key_type VARCHAR(20) DEFAULT 'X25519',
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW(),
    rotated_at TIMESTAMP,
    expires_at TIMESTAMP
);

CREATE INDEX idx_public_keys_user_id ON public_keys(user_id);

-- Audit Log (messenger_audit)
\c messenger_audit;

CREATE TABLE audit_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID,
    action VARCHAR(100) NOT NULL,
    details JSONB,
    ip_address INET,
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_audit_logs_user_id ON audit_logs(user_id);
CREATE INDEX idx_audit_logs_created_at ON audit_logs(created_at);
