# User Service

**Version:** 9.2.2  
**Status:** ✅ Production-Ready  
**Port:** 5003

---

## 📋 Übersicht

Der User Service verwaltet Benutzerprofile, Kontakte und Account-Einstellungen im Secure Messenger System.

---

## 🚀 Features

### ✅ Profile Management
- Get user profile (by ID or current user)
- Update profile (DisplayName, AvatarUrl, Bio)
- Account deletion (30-day grace period)

### ✅ Contact Management
- Add contacts
- List contacts
- Remove contacts
- Search users with contact status

### ✅ Security
- JWT Authentication
- FluentValidation for all inputs
- Input sanitization (XSS protection)
- Master Key Salt retrieval (for E2E encryption)

### ✅ Search
- Search by username, email, or display name
- Pagination support (default: 10, max: 50)
- Contact status in results

---

## 📡 API Endpoints

### **Profile Endpoints**

#### `GET /api/users/{userId}`
Get user profile by ID

**Response:**
```json
{
  "id": "guid",
  "username": "string",
  "email": "string",
  "displayName": "string",
  "avatarUrl": "string",
  "bio": "string",
  "emailVerified": true,
  "createdAt": "2026-01-27T..."
}
```

#### `GET /api/users/me`
Get current user's profile

#### `PUT /api/users/me`
Update current user's profile

**Request:**
```json
{
  "displayName": "New Display Name",
  "avatarUrl": "https://example.com/avatar.jpg",
  "bio": "My new bio"
}
```

**Validation:**
- DisplayName: Max 100 characters
- Bio: Max 500 characters
- AvatarUrl: Valid URL (http/https)

#### `GET /api/users/{userId}/salt`
Get master key salt (for client-side encryption)

**Response:**
```json
{
  "userId": "guid",
  "salt": "base64-encoded-salt"
}
```

---

### **Search Endpoint**

#### `GET /api/users/search?q={query}&page=1&pageSize=10`
Search for users

**Query Parameters:**
- `q` (required): Search query (min 2 characters)
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Results per page (default: 10, max: 50)

**Response:**
```json
[
  {
    "id": "guid",
    "username": "string",
    "displayName": "string",
    "avatarUrl": "string",
    "bio": "string",
    "isContact": false
  }
]
```

---

### **Contact Endpoints**

#### `POST /api/users/contacts`
Add a contact

**Request:**
```json
{
  "contactUserId": "guid",
  "nickname": "Optional nickname"
}
```

**Validation:**
- ContactUserId: Required, must be valid user
- Cannot add yourself as contact
- Cannot add same contact twice
- Nickname: Max 100 characters

#### `GET /api/users/contacts`
List all contacts

**Response:**
```json
[
  {
    "id": "guid",
    "contactUserId": "guid",
    "username": "string",
    "displayName": "string",
    "nickname": "string",
    "avatarUrl": "string",
    "isBlocked": false,
    "addedAt": "2026-01-27T..."
  }
]
```

#### `DELETE /api/users/contacts/{contactId}`
Remove a contact

**Response:** `204 No Content`

---

### **Account Deletion**

#### `DELETE /api/users/me`
Schedule account deletion (30-day grace period)

**Request:**
```json
{
  "password": "user-password",
  "confirmDelete": true
}
```

**Response:**
```json
{
  "message": "Account deletion scheduled. Your account will be deleted in 30 days.",
  "scheduledDate": "2026-02-26T..."
}
```

**Behavior:**
- Account is immediately deactivated (`IsActive = false`)
- Actual deletion happens after 30 days
- User can reactivate account before deletion date

---

## 🛠️ Technologies

- **.NET 8.0**
- **PostgreSQL** (Database)
- **Entity Framework Core 8.0**
- **FluentValidation 11.3**
- **Serilog** (Logging)
- **JWT Bearer Authentication**
- **Swagger/OpenAPI**

---

## 🔧 Configuration

### **appsettings.json**

```json
{
  "ConnectionStrings": {
    "UserDatabase": "Host=postgres;Database=messenger_user;Username=messenger_admin;Password=${POSTGRES_PASSWORD}"
  },
  "Jwt": {
    "Key": "${JWT_SECRET}",
    "Issuer": "MessengerAuthService",
    "Audience": "MessengerClient",
    "ExpiryMinutes": 15
  }
}
```

### **Environment Variables**

Required:
- `POSTGRES_PASSWORD`: Database password
- `JWT_SECRET`: JWT signing key (min 64 chars)

---

## 📊 Database Schema

### **UserProfiles Table**
```sql
CREATE TABLE user_profiles (
    id UUID PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    display_name VARCHAR(100),
    avatar_url VARCHAR(500),
    bio TEXT,
    master_key_salt BYTEA NOT NULL,
    email_verified BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE,
    delete_scheduled_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP
);
```

### **Contacts Table**
```sql
CREATE TABLE contacts (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES user_profiles(id),
    contact_user_id UUID NOT NULL REFERENCES user_profiles(id),
    nickname VARCHAR(100),
    is_blocked BOOLEAN DEFAULT FALSE,
    added_at TIMESTAMP DEFAULT NOW(),
    UNIQUE(user_id, contact_user_id)
);
```

---

## 🧪 Testing

### **Run Tests**
```bash
dotnet test --filter "UserService"
```

### **Test Coverage**
- Unit Tests: 22/22 ✅
- Coverage: ~90%

---

## 🚦 Health Checks

**Endpoint:** `GET /health`

**Response (Healthy):**
```
Healthy
```

**Checks:**
- Database connectivity (PostgreSQL)
- Service status

---

## 📝 Validation Rules

### **UpdateProfileRequest**
- DisplayName: Max 100 characters
- Bio: Max 500 characters
- AvatarUrl: Valid HTTP/HTTPS URL

### **AddContactRequest**
- ContactUserId: Required, valid GUID
- Nickname: Max 100 characters

### **Search Query**
- Min 2 characters
- Max 100 characters

### **DeleteAccountRequest**
- Password: Required
- ConfirmDelete: Must be `true`

---

## 🔒 Security Features

### **Authentication**
- JWT Bearer tokens required (except `/salt` endpoint)
- Token validation on every request

### **Authorization**
- Users can only:
  - View their own profile
  - Update their own profile
  - Manage their own contacts
  - Delete their own account

### **Input Sanitization**
All user inputs are sanitized to prevent XSS:
- `<` → `&lt;`
- `>` → `&gt;`
- `&` → `&amp;`
- `"` → `&quot;`
- `'` → `&#x27;`

### **Rate Limiting**
_(To be implemented)_

---

## 📈 Logging

**Log Levels:**
- `Information`: Normal operations (profile updates, contact management)
- `Warning`: Account deletions, not found errors
- `Error`: Database errors, unexpected exceptions

**Example Logs:**
```
[INFO] User profile {UserId} updated successfully
[WARN] User {UserId} scheduled account deletion for {DeletionDate}
[ERROR] Error getting user profile {UserId}: {Exception}
```

---

## 🚀 Deployment

### **Docker**
```bash
docker build -t user-service .
docker run -p 5003:8080 \
  -e POSTGRES_PASSWORD=xxx \
  -e JWT_SECRET=xxx \
  user-service
```

### **Health Check**
```bash
curl http://localhost:5003/health
```

---

## 📞 Dependencies

### **Internal Services**
- **AuthService**: User authentication (JWT tokens)
- **MessageService**: User references in messages

### **External Dependencies**
- **PostgreSQL**: User data storage
- **Shared Libraries**:
  - `MessengerContracts`: DTOs
  - `MessengerCommon`: Extensions

---

## 🎯 Future Enhancements

- [ ] Email verification workflow
- [ ] Account recovery (undelete within 30 days)
- [ ] Blocked users management
- [ ] Profile privacy settings
- [ ] Rate limiting
- [ ] Redis caching for profiles
- [ ] Audit logging integration

---

## 📚 Related Documentation

- [API Reference](../../docs/09_API_REFERENCE.md)
- [Database Migrations](../../docs/DATABASE_MIGRATIONS.md)
- [Authentication Flow](../../docs/03_CRYPTOGRAPHY.md)

---

**Last Updated:** 2026-01-27  
**Version:** 9.2.2  
**Status:** ✅ **Production-Ready**
