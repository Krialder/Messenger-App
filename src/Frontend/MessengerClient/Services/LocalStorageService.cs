using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MessengerClient.Data;
using Microsoft.EntityFrameworkCore;

namespace MessengerClient.Services
{
    public class LocalStorageService
    {
        private readonly LocalDbContext _context;

        public LocalStorageService(LocalDbContext context)
        {
            _context = context;
        }

        public async Task SaveTokenAsync(string token)
        {
            await File.WriteAllTextAsync("jwt_token.txt", token);
        }

        public async Task<string?> GetTokenAsync()
        {
            if (File.Exists("jwt_token.txt"))
            {
                return await File.ReadAllTextAsync("jwt_token.txt");
            }
            return null;
        }

        public async Task ClearTokenAsync()
        {
            if (File.Exists("jwt_token.txt"))
            {
                File.Delete("jwt_token.txt");
            }
            await Task.CompletedTask;
        }

        // Refresh Token Management (separate storage for security)
        public async Task SaveRefreshTokenAsync(string refreshToken)
        {
            await File.WriteAllTextAsync("refresh_token.txt", refreshToken);
        }

        public async Task<string?> GetRefreshTokenAsync()
        {
            if (File.Exists("refresh_token.txt"))
            {
                return await File.ReadAllTextAsync("refresh_token.txt");
            }
            return null;
        }

        public async Task ClearRefreshTokenAsync()
        {
            if (File.Exists("refresh_token.txt"))
            {
                File.Delete("refresh_token.txt");
            }
            await Task.CompletedTask;
        }

        public async Task SaveUserProfileAsync(Guid userId, string email, string displayName, byte[] salt, byte[] publicKey)
        {
            LocalUserProfile? profile = await _context.UserProfile.FindAsync(userId);
            if (profile == null)
            {
                profile = new LocalUserProfile
                {
                    UserId = userId,
                    Email = email,
                    DisplayName = displayName,
                    Salt = salt,
                    PublicKey = publicKey
                };
                await _context.UserProfile.AddAsync(profile);
            }
            else
            {
                profile.Email = email;
                profile.DisplayName = displayName;
                profile.Salt = salt;
                profile.PublicKey = publicKey;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<LocalUserProfile?> GetUserProfileAsync()
        {
            return await _context.UserProfile.FirstOrDefaultAsync();
        }

        public async Task SaveMessageAsync(LocalMessage message)
        {
            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();
        }

        public async Task<List<LocalMessage>> GetMessagesAsync(Guid conversationId)
        {
            return await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }

        public async Task SaveConversationAsync(LocalConversation conversation)
        {
            LocalConversation? existing = await _context.Conversations.FindAsync(conversation.Id);
            if (existing == null)
            {
                await _context.Conversations.AddAsync(conversation);
            }
            else
            {
                existing.Name = conversation.Name;
                existing.LastMessageAt = conversation.LastMessageAt;
                existing.LastMessagePreview = conversation.LastMessagePreview;
                existing.UnreadCount = conversation.UnreadCount;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<LocalConversation>> GetConversationsAsync()
        {
            return await _context.Conversations
                .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
                .ToListAsync();
        }

        public async Task SaveContactAsync(LocalContact contact)
        {
            LocalContact? existing = await _context.Contacts.FirstOrDefaultAsync(c => c.UserId == contact.UserId);
            if (existing == null)
            {
                await _context.Contacts.AddAsync(contact);
            }
            else
            {
                existing.DisplayName = contact.DisplayName;
                existing.Email = contact.Email;
                existing.AvatarUrl = contact.AvatarUrl;
                existing.IsOnline = contact.IsOnline;
                existing.LastSeen = contact.LastSeen;
                existing.PublicKey = contact.PublicKey;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<LocalContact>> GetContactsAsync()
        {
            return await _context.Contacts
                .OrderBy(c => c.DisplayName)
                .ToListAsync();
        }

        public async Task SaveKeyPairAsync(byte[] publicKey, byte[] privateKey)
        {
            LocalKeyPair keyPair = new LocalKeyPair
            {
                Id = Guid.NewGuid(),
                PublicKey = publicKey,
                PrivateKey = privateKey,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _context.KeyPairs.Where(k => k.IsActive).ForEachAsync(k => k.IsActive = false);
            await _context.KeyPairs.AddAsync(keyPair);
            await _context.SaveChangesAsync();
        }

        public async Task<LocalKeyPair?> GetActiveKeyPairAsync()
        {
            return await _context.KeyPairs
                .Where(k => k.IsActive)
                .OrderByDescending(k => k.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}
