using Refit;
using MessengerContracts.DTOs;

namespace MessengerClient.Services
{
    public interface IMessageApiService
    {
        [Post("/api/messages")]
        Task<MessageResponse> SendMessageAsync([Header("Authorization")] string authorization, [Body] SendMessageRequest request);

        [Get("/api/messages/conversations")]
        Task<List<ConversationDto>> GetConversationsAsync([Header("Authorization")] string authorization);

        [Get("/api/messages/conversations/{conversationId}")]
        Task<List<MessageDto>> GetMessagesAsync([Header("Authorization")] string authorization, Guid conversationId);

        [Post("/api/messages/groups")]
        Task<ConversationDto> CreateGroupAsync([Header("Authorization")] string authorization, [Body] CreateGroupRequest request);

        [Post("/api/messages/groups/{groupId}/members")]
        Task AddGroupMemberAsync([Header("Authorization")] string authorization, Guid groupId, [Body] AddGroupMemberRequest request);

        [Delete("/api/messages/{messageId}")]
        Task DeleteMessageAsync([Header("Authorization")] string authorization, Guid messageId);
    }
}
