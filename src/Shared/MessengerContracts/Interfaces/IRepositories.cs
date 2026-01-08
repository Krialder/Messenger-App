using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessengerContracts.DTOs;

namespace MessengerContracts.Interfaces
{
    /// <summary>
    /// Interface for message repository
    /// </summary>
    public interface IMessageRepository
    {
        Task<MessageDto> CreateAsync(MessageDto message);
        Task<MessageDto?> GetByIdAsync(Guid id);
        Task<List<MessageDto>> GetConversationAsync(Guid userId, Guid contactId, int page = 1, int pageSize = 50);
        Task UpdateStatusAsync(Guid messageId, MessageStatus status);
        Task SoftDeleteAsync(Guid messageId);
        Task HardDeleteAsync(Guid messageId);
    }

    /// <summary>
    /// Interface for user repository
    /// </summary>
    public interface IUserRepository
    {
        Task<UserDto> CreateAsync(RegisterUserDto user, byte[] passwordHash, byte[] masterKeySalt);
        Task<UserDto?> GetByIdAsync(Guid id);
        Task<UserDto?> GetByUsernameAsync(string username);
        Task<UserDto?> GetByEmailAsync(string email);
        Task UpdateOnlineStatusAsync(Guid userId, bool isOnline);
        Task DeleteAsync(Guid userId);
    }

    /// <summary>
    /// Interface for key repository
    /// </summary>
    public interface IKeyRepository
    {
        Task<byte[]> GetPublicKeyAsync(Guid userId);
        Task StorePublicKeyAsync(Guid userId, byte[] publicKey, string keyType);
        Task<List<byte[]>> GetActiveKeysAsync(Guid userId);
        Task RevokeKeyAsync(Guid keyId);
    }

    /// <summary>
    /// Interface for MFA repository
    /// </summary>
    public interface IMfaRepository
    {
        Task<List<MfaMethodDto>> GetMethodsAsync(Guid userId);
        Task<MfaMethodDto> AddMethodAsync(Guid userId, MfaMethodType methodType, string secret);
        Task<bool> VerifyMethodAsync(Guid userId, MfaMethodType methodType, string code);
        Task DisableMethodAsync(Guid methodId);
    }
}
