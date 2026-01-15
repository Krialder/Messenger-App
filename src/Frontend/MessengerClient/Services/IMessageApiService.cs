using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using MessengerContracts.DTOs;

namespace MessengerClient.Services
{
    public interface IMessageApiService
    {
        [Post("/api/messages")]
        Task<MessengerContracts.DTOs.MessageDto> SendMessageAsync([Body] MessageServiceSendMessageRequest request, [Header("Authorization")] string authorization);

        [Get("/api/messages/conversations")]
        Task<List<MessengerContracts.DTOs.ConversationDto>> GetConversationsAsync([Header("Authorization")] string authorization);

        [Get("/api/messages/conversation/{conversationId}")]
        Task<List<MessengerContracts.DTOs.MessageDto>> GetMessagesAsync(Guid conversationId, [Header("Authorization")] string authorization);

        [Post("/api/groups")]
        Task<MessengerContracts.DTOs.ConversationDto> CreateGroupAsync([Body] CreateGroupRequest request, [Header("Authorization")] string authorization);

        [Post("/api/groups/{groupId}/members")]
        Task AddGroupMemberAsync(Guid groupId, [Body] AddGroupMemberRequest request, [Header("Authorization")] string authorization);

        [Delete("/api/messages/{messageId}")]
        Task DeleteMessageAsync(Guid messageId, [Header("Authorization")] string authorization);

        [Patch("/api/messages/{messageId}/read")]
        Task MarkAsReadAsync(Guid messageId, [Header("Authorization")] string authorization);

        [Get("/api/messages/search")]
        Task<List<MessengerContracts.DTOs.MessageDto>> SearchMessagesAsync([Query] string query, [Header("Authorization")] string authorization);
    }
}

namespace MessengerClient.Services
{
    // Supporting DTOs - renamed to avoid conflicts
    public record MessageServiceSendMessageRequest(
        Guid ConversationId,
        string EncryptedContent,
        string? EncryptedFileId = null
    );

    public record CreateGroupRequest(
        string Name,
        List<Guid> MemberIds
    );

    public record AddGroupMemberRequest(
        Guid UserId
    );
}
