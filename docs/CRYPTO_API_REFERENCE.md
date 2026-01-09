# CryptoService API Reference - Version 7.0

**Purpose**: Complete API documentation for Integration Tests

---

## **Layer 1: TransportEncryptionService**

**Namespace**: `CryptoService.Layer1`  
**Purpose**: End-to-End Transport Encryption (ChaCha20-Poly1305 + X25519)

### **Methods:**

#### `GenerateKeyPairAsync()`
```csharp
Task<KeyPair> GenerateKeyPairAsync()
```
**Returns**: `MessengerContracts.DTOs.KeyPair`
- `PublicKey`: byte[] (32 bytes)
- `PrivateKey`: byte[] (32 bytes)

**Example**:
```csharp
var keyPair = await _transportService.GenerateKeyPairAsync();
// keyPair.PublicKey: byte[32]
// keyPair.PrivateKey: byte[32]
```

---

#### `EncryptAsync()`
```csharp
Task<EncryptedMessageDto> EncryptAsync(string plaintext, byte[] recipientPublicKey)
```
**Parameters**:
- `plaintext`: string - Message to encrypt
- `recipientPublicKey`: byte[32] - Recipient's X25519 public key

**Returns**: `EncryptedMessageDto`
- `Nonce`: byte[24]
- `Ciphertext`: byte[]
- `Tag`: byte[16]
- `EphemeralPublicKey`: byte[32]

**Example**:
```csharp
var encrypted = await _transportService.EncryptAsync("Hello!", recipientPublicKey);
// encrypted.Nonce: byte[24]
// encrypted.Ciphertext: byte[]
// encrypted.Tag: byte[16]
// encrypted.EphemeralPublicKey: byte[32]
```

---

#### `DecryptAsync()`
```csharp
Task<string> DecryptAsync(EncryptedMessageDto encryptedMessage, byte[] privateKey)
```
**Parameters**:
- `encryptedMessage`: EncryptedMessageDto
- `privateKey`: byte[32] - Recipient's private key

**Returns**: `string` - Decrypted plaintext

**Example**:
```csharp
var plaintext = await _transportService.DecryptAsync(encrypted, myPrivateKey);
// plaintext: "Hello!"
```

---

## **Layer 2: LocalStorageEncryptionService**

**Namespace**: `CryptoService.Layer2`  
**Purpose**: Local Storage Encryption (AES-256-GCM + Argon2id)

### **Methods:**

#### `DeriveMasterKeyAsync()`
```csharp
Task<byte[]> DeriveMasterKeyAsync(string password, byte[] userSalt)
```
**Parameters**:
- `password`: string - User's password
- `userSalt`: byte[32] - User-specific salt

**Returns**: `byte[32]` - Master key

**Performance**: ~100-200ms (intentionally slow)

**Example**:
```csharp
var salt = new byte[32];
RandomNumberGenerator.Fill(salt);
var masterKey = await _layer2Service.DeriveMasterKeyAsync("password123", salt);
// masterKey: byte[32]
```

---

#### `EncryptLocalDataAsync()`
```csharp
Task<string> EncryptLocalDataAsync(string plaintext, byte[] masterKey)
```
**Parameters**:
- `plaintext`: string - Data to encrypt
- `masterKey`: byte[32] - Master key from DeriveMasterKeyAsync

**Returns**: `string` - Base64-encoded encrypted data (nonce || ciphertext || tag)

**Performance**: < 0.5ms

**Example**:
```csharp
var encrypted = await _layer2Service.EncryptLocalDataAsync("secret data", masterKey);
// encrypted: "base64string..."
```

---

#### `DecryptLocalDataAsync()`
```csharp
Task<string> DecryptLocalDataAsync(string encryptedData, byte[] masterKey)
```
**Parameters**:
- `encryptedData`: string - Base64-encoded encrypted data
- `masterKey`: byte[32] - Master key

**Returns**: `string` - Decrypted plaintext

**Example**:
```csharp
var plaintext = await _layer2Service.DecryptLocalDataAsync(encrypted, masterKey);
// plaintext: "secret data"
```

---

## **GroupEncryptionService**

**Namespace**: `CryptoService.Services`  
**Purpose**: Group Message Encryption (Signal Protocol)

### **Methods:**

#### `GenerateNewGroupKeyAsync()`
```csharp
Task<byte[]> GenerateNewGroupKeyAsync()
```
**Returns**: `byte[32]` - Random AES-256 group key

**Example**:
```csharp
var groupKey = await _groupService.GenerateNewGroupKeyAsync();
// groupKey: byte[32]
```

---

#### `EncryptForGroupAsync()`
```csharp
Task<GroupMessageEncryptionResult> EncryptForGroupAsync(
    string plaintext,
    List<MemberPublicKey> memberPublicKeys)
```
**Parameters**:
- `plaintext`: string - Message to encrypt
- `memberPublicKeys`: List<MemberPublicKey> - All group members' public keys

**Returns**: `GroupMessageEncryptionResult`
- `EncryptedContent`: byte[]
- `Nonce`: byte[12]
- `Tag`: byte[16]
- `EncryptedKeys`: List<EncryptedKeyPair>

**Example**:
```csharp
var members = new List<MemberPublicKey>
{
    new MemberPublicKey { UserId = user1Id, PublicKey = pubKey1 },
    new MemberPublicKey { UserId = user2Id, PublicKey = pubKey2 }
};

var result = await _groupService.EncryptForGroupAsync("Group message!", members);
// result.EncryptedContent: byte[]
// result.EncryptedKeys.Count: 2
```

---

#### `DecryptGroupMessageAsync()`
```csharp
Task<string> DecryptGroupMessageAsync(
    byte[] encryptedContent,
    byte[] nonce,
    byte[] tag,
    byte[] encryptedGroupKey,
    byte[] myPrivateKey)
```
**Parameters**:
- `encryptedContent`: byte[] - Encrypted message
- `nonce`: byte[12] - AES-GCM nonce
- `tag`: byte[16] - Authentication tag
- `encryptedGroupKey`: byte[] - Group key encrypted for me
- `myPrivateKey`: byte[32] - My private key

**Returns**: `string` - Decrypted plaintext

**Example**:
```csharp
var plaintext = await _groupService.DecryptGroupMessageAsync(
    result.EncryptedContent,
    result.Nonce,
    result.Tag,
    result.EncryptedKeys[0].EncryptedGroupKey,
    myPrivateKey);
// plaintext: "Group message!"
```

---

## **Supporting Types**

### **KeyPair** (MessengerContracts.DTOs)
```csharp
public class KeyPair
{
    public byte[] PublicKey { get; set; }
    public byte[] PrivateKey { get; set; }
}
```

### **EncryptedMessageDto**
```csharp
public class EncryptedMessageDto
{
    public byte[] Nonce { get; set; }           // 24 bytes
    public byte[] Ciphertext { get; set; }
    public byte[] Tag { get; set; }             // 16 bytes
    public byte[] EphemeralPublicKey { get; set; } // 32 bytes
}
```

### **MemberPublicKey**
```csharp
public class MemberPublicKey
{
    public Guid UserId { get; set; }
    public byte[] PublicKey { get; set; }
}
```

### **GroupMessageEncryptionResult**
```csharp
public class GroupMessageEncryptionResult
{
    public byte[] EncryptedContent { get; set; }
    public byte[] Nonce { get; set; }
    public byte[] Tag { get; set; }
    public List<EncryptedKeyPair> EncryptedKeys { get; set; }
}
```

### **EncryptedKeyPair**
```csharp
public class EncryptedKeyPair
{
    public Guid UserId { get; set; }
    public byte[] EncryptedGroupKey { get; set; }
}
```

---

## **Performance Targets**

| Operation | Target | Service |
|-----------|--------|---------|
| **GenerateKeyPairAsync** | < 10ms | Layer 1 |
| **EncryptAsync** (Layer 1) | < 10ms | Layer 1 |
| **DecryptAsync** (Layer 1) | < 10ms | Layer 1 |
| **DeriveMasterKeyAsync** | 100-200ms | Layer 2 (intentionally slow) |
| **EncryptLocalDataAsync** | < 0.5ms | Layer 2 |
| **DecryptLocalDataAsync** | < 0.5ms | Layer 2 |
| **EncryptForGroupAsync** (256 members) | < 5000ms | Group |
| **DecryptGroupMessageAsync** | < 10ms | Group |

---

**Version**: 7.0  
**Last Updated**: 2025-01-09  
**Status**: ✅ Complete - Ready for Integration Tests
