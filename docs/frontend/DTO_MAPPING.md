# DTO Mapping Reference - Frontend ↔ Backend

## 📋 **DTO Compatibility Check**

Dieses Dokument mappt die in der Frontend-Implementierung verwendeten DTOs zu den tatsächlichen DTOs in `MessengerContracts`.

---

## ✅ **Authentication DTOs**

### **Frontend → Backend Mapping**

| Frontend (Used in Services) | Backend (MessengerContracts) | Status | Notes |
|------------------------------|------------------------------|--------|-------|
| `AuthResponse` | `LoginResponse` | ⚠️ **Rename Needed** | Used in IAuthApiService |
| `RegisterRequest` | `RegisterRequest` | ✅ **Match** | OK |
| `LoginRequest` | `LoginRequest` | ✅ **Match** | OK |
| `VerifyMfaRequest` | `VerifyMfaRequest` | ✅ **Match** | OK |
| `RefreshTokenRequest` | `RefreshTokenRequest` | ✅ **Match** | OK |
| `MfaSetupResponse` | `EnableTotpResponse` | ⚠️ **Rename Needed** | QrCodeUrl property |

---

## 🔧 **Required Frontend Changes**

### **1. IAuthApiService.cs** - Update Return Types

**BEFORE** (Current):
```csharp
Task<AuthResponse> RegisterAsync([Body] RegisterRequest request);
Task<AuthResponse> LoginAsync([Body] LoginRequest request);
Task<MfaSetupResponse> SetupMfaAsync([Header("Authorization")] string authorization);
Task<AuthResponse> VerifyMfaAsync([Body] VerifyMfaRequest request);
Task<AuthResponse> RefreshTokenAsync([Body] RefreshTokenRequest request);
```

**AFTER** (Fixed):
```csharp
Task<RegisterResponse> RegisterAsync([Body] RegisterRequest request);
Task<LoginResponse> LoginAsync([Body] LoginRequest request);
Task<EnableTotpResponse> SetupMfaAsync([Header("Authorization")] string authorization);
Task<LoginResponse> VerifyMfaAsync([Body] VerifyMfaRequest request);
Task<TokenResponse> RefreshTokenAsync([Body] RefreshTokenRequest request);
```

### **2. LoginViewModel.cs** - Update Property Access

**BEFORE**:
```csharp
AuthResponse response = await _authApi.LoginAsync(request);
await _localStorage.SaveTokenAsync(response.Token);
await _signalR.ConnectAsync(response.Token);
```

**AFTER**:
```csharp
LoginResponse response = await _authApi.LoginAsync(request);
await _localStorage.SaveTokenAsync(response.AccessToken);
await _signalR.ConnectAsync(response.AccessToken);

// Access user data:
// response.User.Id
// response.User.Username
// response.User.Email
```

### **3. RegisterViewModel.cs** - Update Return Type

**BEFORE**:
```csharp
AuthResponse response = await _authApi.RegisterAsync(request);
await _localStorage.SaveUserProfileAsync(response.UserId, Email, DisplayName, salt, response.PublicKey);
```

**AFTER**:
```csharp
RegisterResponse response = await _authApi.RegisterAsync(request);
await _localStorage.SaveUserProfileAsync(
    response.UserId, 
    Email, 
    DisplayName, 
    salt, 
    Convert.FromBase64String(response.MasterKeySalt) // Salt is Base64 encoded
);
```

### **4. SettingsViewModel.cs** - Update MFA Setup

**BEFORE**:
```csharp
MfaSetupResponse response = await _authApi.SetupMfaAsync($"Bearer {token}");
QrCodeUri = response.QrCodeUri;
```

**AFTER**:
```csharp
EnableTotpResponse response = await _authApi.SetupMfaAsync($"Bearer {token}");
QrCodeUri = response.QrCodeUrl; // Property name is 'QrCodeUrl', not 'QrCodeUri'
SecretKey = response.Secret;
BackupCodes = response.BackupCodes; // Display these once!
```

---

## ✅ **Message DTOs**

### **Frontend → Backend Mapping**

| Frontend (Used in Services) | Backend (MessengerContracts) | Status | Notes |
|------------------------------|------------------------------|--------|-------|
| `SendMessageRequest` | `SendMessageRequest` | ✅ **Match** | OK |
| `MessageResponse` | `MessageResponse` | ✅ **Match** | OK |
| `MessageDto` | `MessageDto` | ✅ **Match** | OK |
| `ConversationDto` | `ConversationDto` | ✅ **Match** | OK |
| `CreateGroupRequest` | `CreateGroupRequest` | ✅ **Match** | OK |
| `AddGroupMemberRequest` | `AddGroupMemberRequest` | ✅ **Match** | OK |

**All Message DTOs are compatible** ✅

---

## ✅ **User DTOs**

### **Frontend → Backend Mapping**

| Frontend (Used in Services) | Backend (MessengerContracts) | Status | Notes |
|------------------------------|------------------------------|--------|-------|
| `UserProfileDto` | `UserDto` | ⚠️ **Rename Needed** | Property names might differ |
| `UpdateProfileRequest` | `UpdateProfileRequest` | ✅ **Match** | OK |
| `ContactDto` | `ContactDto` | ✅ **Match** | OK |
| `AddContactRequest` | `AddContactRequest` | ✅ **Match** | OK |

### **Required Changes**

**IUserApiService.cs**:
```csharp
// BEFORE
Task<UserProfileDto> GetProfileAsync([Header("Authorization")] string authorization);

// AFTER
Task<UserDto> GetProfileAsync([Header("Authorization")] string authorization);
```

**ContactsViewModel.cs**:
```csharp
// BEFORE
List<UserProfileDto> users = await _userApi.SearchUsersAsync($"Bearer {_jwtToken}", SearchQuery);

// AFTER
List<UserDto> users = await _userApi.SearchUsersAsync($"Bearer {_jwtToken}", SearchQuery);
```

---

## ✅ **File DTOs**

### **Frontend → Backend Mapping**

| Frontend (Used in Services) | Backend (MessengerContracts) | Status | Notes |
|------------------------------|------------------------------|--------|-------|
| `FileUploadResponse` | `FileUploadResponse` | ✅ **Match** | OK |
| `FileDownloadResponse` | `FileDownloadResponse` | ✅ **Match** | OK |
| `FileMetadataDto` | `FileMetadataDto` | ✅ **Match** | OK |

**All File DTOs are compatible** ✅

---

## ✅ **Crypto DTOs**

### **Frontend → Backend Mapping**

| Frontend (Used in Services) | Backend (MessengerContracts) | Status | Notes |
|------------------------------|------------------------------|--------|-------|
| `KeyPairResponse` | `KeyPair` | ⚠️ **Check** | Might be `GenerateKeyPairResponse` |
| `EncryptRequest` | `EncryptRequest` | ✅ **Match** | OK |
| `DecryptRequest` | `DecryptRequest` | ✅ **Match** | OK |
| `EncryptedMessageDto` | `EncryptedMessageDto` | ✅ **Match** | OK |
| `DecryptResponse` | `DecryptResponse` | ✅ **Match** | OK |
| `GroupEncryptRequest` | `GroupEncryptRequest` | ✅ **Match** | OK |
| `GroupDecryptRequest` | `GroupDecryptRequest` | ✅ **Match** | OK |
| `GroupMessageEncryptionResult` | `GroupMessageEncryptionResult` | ✅ **Match** | OK |

**Most Crypto DTOs are compatible** ✅

---

## 🔧 **Frontend Files to Update**

### **Priority 1: Service Interfaces**

1. **`IAuthApiService.cs`** ⚠️
   - Change `AuthResponse` → `LoginResponse` / `RegisterResponse` / `TokenResponse`
   - Change `MfaSetupResponse` → `EnableTotpResponse`

2. **`IUserApiService.cs`** ⚠️
   - Change `UserProfileDto` → `UserDto`

3. **`ICryptoApiService.cs`** ⚠️
   - Change `KeyPairResponse` → Check actual DTO name (might be `KeyPair`)

### **Priority 2: ViewModels**

1. **`LoginViewModel.cs`** ⚠️
   - Change `AuthResponse` → `LoginResponse`
   - Change `response.Token` → `response.AccessToken`
   - Add `response.RefreshToken` handling

2. **`RegisterViewModel.cs`** ⚠️
   - Change `AuthResponse` → `RegisterResponse`
   - Change `response.PublicKey` → Parse from `response.MasterKeySalt` (Base64)

3. **`SettingsViewModel.cs`** ⚠️
   - Change `MfaSetupResponse` → `EnableTotpResponse`
   - Change `QrCodeUri` → `QrCodeUrl`
   - Add `BackupCodes` handling

4. **`ContactsViewModel.cs`** ⚠️
   - Change `UserProfileDto` → `UserDto`

---

## ✅ **Summary**

### **DTOs to Fix** (Total: 4)

1. ✅ **AuthResponse** → `LoginResponse` / `RegisterResponse` / `TokenResponse`
2. ✅ **MfaSetupResponse** → `EnableTotpResponse`
3. ✅ **UserProfileDto** → `UserDto`
4. ✅ **KeyPairResponse** → Check actual DTO name

### **Files to Update** (Total: 6)

**Service Interfaces**:
1. ⚠️ `IAuthApiService.cs` (3 return types)
2. ⚠️ `IUserApiService.cs` (1 return type)
3. ⚠️ `ICryptoApiService.cs` (1 return type)

**ViewModels**:
4. ⚠️ `LoginViewModel.cs` (Property access)
5. ⚠️ `RegisterViewModel.cs` (Property access)
6. ⚠️ `SettingsViewModel.cs` (Property access)
7. ⚠️ `ContactsViewModel.cs` (Type change)

---

## 📋 **Action Items**

**BEFORE starting XAML**:
1. ✅ Fix `IAuthApiService.cs` return types
2. ✅ Fix `IUserApiService.cs` return types
3. ✅ Fix `ICryptoApiService.cs` return types
4. ✅ Update all ViewModels to use correct DTO properties
5. ✅ Build & Test (ensure no compilation errors)

**THEN**:
6. ⏳ Start XAML implementation (QUICK_START.md)

---

**Version**: 8.0  
**Last Updated**: 2025-01-10  
**Status**: ⚠️ **DTOs Need Mapping Fix Before XAML**

**Priority**: **HIGH** - Fix DTOs before XAML implementation
