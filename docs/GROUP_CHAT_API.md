# Group Chat API Documentation

> **Status**: ✅ Implemented - Ready for testing

## Base URL

```
Development: http://localhost:5002/api
Production:  https://api.secure-messenger.com/v1
```

## Authentication

All endpoints require JWT authentication via `Authorization: Bearer {token}` header.

---

## 1. Group Management

### 1.1 Create Group

Create a new group conversation.

**Endpoint**: `POST /groups`

**Request**:
```json
{
  "name": "Project Team",
  "description": "Main project discussion group",
  "memberIds": [
    "user-uuid-1",
    "user-uuid-2",
    "user-uuid-3"
  ]
}
```

**Response** (201 Created):
```json
{
  "groupId": "conversation-uuid",
  "memberCount": 4
}
```

**Validation**:
- Name: Required, 1-100 characters
- Description: Optional, max 500 characters
- MemberIds: At least 1 other member, max 256 total members

**Errors**:
- `400` - Validation failed (empty name, too many members)
- `401` - Unauthorized (invalid JWT)

---

### 1.2 Get Group Details

Retrieve group information including all members.

**Endpoint**: `GET /groups/{id}`

**Response** (200 OK):
```json
{
  "id": "conversation-uuid",
  "type": 1,
  "name": "Project Team",
  "description": "Main project discussion group",
  "avatarUrl": null,
  "createdAt": "2025-01-07T10:00:00Z",
  "createdBy": "user-uuid-owner",
  "members": [
    {
      "id": "member-uuid-1",
      "userId": "user-uuid-1",
      "username": "alice",
      "role": 2,
      "joinedAt": "2025-01-07T10:00:00Z",
      "leftAt": null,
      "nickname": null,
      "notificationsEnabled": true
    },
    {
      "id": "member-uuid-2",
      "userId": "user-uuid-2",
      "username": "bob",
      "role": 0,
      "joinedAt": "2025-01-07T10:00:00Z",
      "leftAt": null,
      "nickname": "Bobby",
      "notificationsEnabled": true
    }
  ],
  "lastMessage": null,
  "unreadCount": 0
}
```

**Member Roles**:
- `0` = Member (can send/read messages)
- `1` = Admin (can manage members and settings)
- `2` = Owner (cannot be removed, full permissions)

**Errors**:
- `404` - Group not found
- `403` - Not a member of this group

---

### 1.3 Add Members

Add new members to a group.

**Endpoint**: `POST /groups/{id}/members`

**Request**:
```json
{
  "userIds": [
    "user-uuid-4",
    "user-uuid-5"
  ],
  "role": 0
}
```

**Response** (204 No Content)

**Permissions**: Only Admins and Owners can add members

**Errors**:
- `400` - Maximum 256 members exceeded
- `403` - Insufficient permissions (only Member role)
- `404` - Group not found

---

### 1.4 Remove Member

Remove a member from the group.

**Endpoint**: `DELETE /groups/{id}/members/{userId}`

**Response** (204 No Content)

**Rules**:
- Admins/Owners can remove others
- Any member can remove themselves (leave group)
- Cannot remove the Owner

**Side Effects**:
- Member's `left_at` timestamp is set (soft delete)
- **TODO**: Group key rotation triggered for security

**Errors**:
- `400` - Cannot remove owner
- `403` - Insufficient permissions
- `404` - Member not found in group

---

### 1.5 Update Group Settings

Update group name, description, or avatar.

**Endpoint**: `PATCH /groups/{id}`

**Request**:
```json
{
  "name": "Updated Project Team",
  "description": "New description",
  "avatarUrl": "https://cdn.example.com/group-avatar.jpg"
}
```

**Response** (204 No Content)

**Permissions**: Only Admins and Owners can update settings

**Errors**:
- `403` - Insufficient permissions
- `404` - Group not found

---

### 1.6 Leave Group

Leave a group conversation.

**Endpoint**: `POST /groups/{id}/leave`

**Response** (204 No Content)

**Rules**:
- Owner cannot leave (must transfer ownership first)
- Sets `left_at` timestamp
- Removes access to future messages

**Errors**:
- `400` - Owner cannot leave group
- `404` - Not a member of this group

---

## 2. Messaging

### 2.1 Send Message (Direct or Group)

Send an encrypted message to a conversation.

**Endpoint**: `POST /messages`

**Request (Direct Message)**:
```json
{
  "conversationId": "conversation-uuid",
  "encryptedContent": "base64-encoded-ciphertext",
  "nonce": "base64-encoded-nonce",
  "ephemeralPublicKey": "base64-encoded-key",
  "type": 0
}
```

**Request (Group Message)**:
```json
{
  "conversationId": "conversation-uuid",
  "encryptedContent": "base64-encoded-group-encrypted-content",
  "nonce": "base64-encoded-nonce",
  "ephemeralPublicKey": null,
  "encryptedGroupKeys": [
    {
      "userId": "user-uuid-1",
      "encryptedGroupKey": "base64-encoded-key-for-user-1"
    },
    {
      "userId": "user-uuid-2",
      "encryptedGroupKey": "base64-encoded-key-for-user-2"
    }
  ],
  "type": 0,
  "replyToMessageId": null
}
```

**Response** (200 OK):
```json
{
  "messageId": "message-uuid",
  "timestamp": "2025-01-07T12:34:56Z",
  "status": "Sent"
}
```

**Message Types**:
- `0` = Text
- `1` = Image
- `2` = File
- `3` = Voice
- `4` = Video
- `5` = System Notification

**Errors**:
- `403` - Not a member of conversation
- `404` - Conversation not found

---

### 2.2 Get Conversation Messages

Retrieve message history for a conversation (paginated).

**Endpoint**: `GET /messages/conversation/{conversationId}`

**Query Parameters**:
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Messages per page (default: 50, max: 100)

**Response** (200 OK):
```json
{
  "messages": [
    {
      "id": "message-uuid",
      "conversationId": "conversation-uuid",
      "senderId": "user-uuid",
      "encryptedContent": "base64-encoded-ciphertext",
      "nonce": "base64-encoded-nonce",
      "ephemeralPublicKey": "base64-encoded-key",
      "timestamp": "2025-01-07T12:34:56Z",
      "status": 2,
      "isDeleted": false,
      "type": 0,
      "replyToMessageId": null
    }
  ],
  "totalCount": 150,
  "page": 1,
  "pageSize": 50,
  "hasMore": true
}
```

**Errors**:
- `403` - Not a member of conversation
- `404` - Conversation not found

---

### 2.3 Get Unread Message Count

Get unread message counts for all conversations.

**Endpoint**: `GET /messages/unread`

**Response** (200 OK):
```json
{
  "unreadConversations": 2,
  "conversations": [
    {
      "conversationId": "conversation-uuid-1",
      "unreadCount": 5
    },
    {
      "conversationId": "conversation-uuid-2",
      "unreadCount": 12
    }
  ]
}
```

---

### 2.4 Mark Conversation as Read

Mark all messages in a conversation as read.

**Endpoint**: `PUT /messages/conversation/{conversationId}/read`

**Response** (204 No Content)

**Side Effects**:
- Updates member's `last_read_at` timestamp
- **TODO**: Emits SignalR event to other members

---

### 2.5 Delete Message

Delete a message (soft delete).

**Endpoint**: `DELETE /messages/{messageId}`

**Response** (204 No Content)

**Rules**:
- Only sender can delete their own messages
- Message marked with `is_deleted = true` and `deleted_at` timestamp
- Deleted messages still exist in database (for audit)

**Errors**:
- `403` - Can only delete your own messages
- `404` - Message not found

---

## 3. Real-time Events (SignalR)

### Connection

**Hub URL**: `wss://localhost:5002/hubs/notifications`

**Authentication**: JWT via query string
```
wss://.../hubs/notifications?access_token={accessToken}
```

### Client Methods (Invoked by client)

#### Join Conversation Group
```javascript
await connection.invoke("JoinConversationGroup", "conversation-uuid");
```

#### Leave Conversation Group
```javascript
await connection.invoke("LeaveConversationGroup", "conversation-uuid");
```

#### Send Group Typing Indicator
```javascript
await connection.invoke("SendGroupTypingIndicator", "conversation-uuid");
```

### Server Events (Received by client)

#### NewMessage
```json
{
  "messageId": "message-uuid",
  "conversationId": "conversation-uuid",
  "senderId": "user-uuid",
  "timestamp": "2025-01-07T12:34:56Z"
}
```

#### GroupTypingIndicator
```json
{
  "conversationId": "conversation-uuid",
  "senderId": "user-uuid"
}
```

#### MemberJoined
```json
{
  "conversationId": "conversation-uuid",
  "memberId": "user-uuid",
  "memberName": "Alice"
}
```

#### MemberLeft
```json
{
  "conversationId": "conversation-uuid",
  "memberId": "user-uuid",
  "memberName": "Bob"
}
```

#### GroupUpdated
```json
{
  "conversationId": "conversation-uuid",
  "updatedData": {
    "name": "New Group Name",
    "description": "Updated description"
  }
}
```

---

## 4. Encryption

### Group Message Encryption (Signal Protocol Approach)

**Client-side Flow**:

1. **Generate ephemeral group key** (32 bytes, AES-256)
   ```javascript
   const groupKey = crypto.getRandomValues(new Uint8Array(32));
   ```

2. **Encrypt message with group key**
   ```javascript
   const { ciphertext, nonce, tag } = await aesGcmEncrypt(plaintext, groupKey);
   ```

3. **Encrypt group key for each member**
   ```javascript
   const encryptedKeys = [];
   for (const member of members) {
     const encryptedGroupKey = await x25519Encrypt(groupKey, member.publicKey);
     encryptedKeys.push({
       userId: member.id,
       encryptedGroupKey: base64(encryptedGroupKey)
     });
   }
   ```

4. **Send to API**
   ```javascript
   await fetch('/api/messages', {
     method: 'POST',
     body: JSON.stringify({
       conversationId,
       encryptedContent: base64(ciphertext),
       nonce: base64(nonce),
       encryptedGroupKeys
     })
   });
   ```

**Decryption Flow**:

1. **Receive message from API/SignalR**
2. **Find your encrypted group key**
   ```javascript
   const myEncryptedKey = message.encryptedGroupKeys.find(k => k.userId === myUserId);
   ```
3. **Decrypt group key with your private key**
   ```javascript
   const groupKey = await x25519Decrypt(myEncryptedKey.encryptedGroupKey, myPrivateKey);
   ```
4. **Decrypt message content**
   ```javascript
   const plaintext = await aesGcmDecrypt(message.encryptedContent, groupKey, message.nonce);
   ```

---

## 5. Error Responses

### Standard Error Format

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Group name is required"
}
```

### HTTP Status Codes

| Code | Meaning |
|------|---------|
| `200` | OK |
| `201` | Created |
| `204` | No Content |
| `400` | Bad Request (validation failed) |
| `401` | Unauthorized (invalid/missing JWT) |
| `403` | Forbidden (insufficient permissions) |
| `404` | Not Found |
| `500` | Internal Server Error |

---

**Document Version**: 1.0  
**Last Updated**: 2025-01-07  
**Status**: ✅ Implemented - Ready for integration testing
