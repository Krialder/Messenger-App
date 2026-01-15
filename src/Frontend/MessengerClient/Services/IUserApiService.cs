using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using MessengerContracts.DTOs;

namespace MessengerClient.Services
{
    public interface IUserApiService
    {
        [Get("/api/users/{userId}")]
        Task<UserDto> GetProfileAsync(Guid userId, [Header("Authorization")] string authorization);

        [Get("/api/users/me")]
        Task<UserDto> GetMyProfileAsync([Header("Authorization")] string authorization);

        [Put("/api/users/me")]
        Task<UserDto> UpdateProfileAsync([Body] UpdateProfileRequest request, [Header("Authorization")] string authorization);

        [Get("/api/users/search")]
        Task<List<UserDto>> SearchUsersAsync([Query] string query, [Header("Authorization")] string authorization);

        [Get("/api/users/me/contacts")]
        Task<List<ContactDto>> GetContactsAsync([Header("Authorization")] string authorization);

        [Post("/api/users/me/contacts")]
        Task<ContactDto> AddContactAsync([Body] AddContactRequest request, [Header("Authorization")] string authorization);

        [Delete("/api/users/me/contacts/{contactId}")]
        Task DeleteContactAsync(Guid contactId, [Header("Authorization")] string authorization);

        [Get("/api/users/{userId}/salt")]
        Task<string> GetMasterKeySaltAsync(Guid userId, [Header("Authorization")] string authorization);
    }

    // Supporting DTOs
    public record UpdateProfileRequest(
        string? DisplayName = null,
        string? Bio = null,
        string? AvatarUrl = null
    );

    public record AddContactRequest(
        Guid UserId,
        string? DisplayName = null
    );

    public record ContactDto
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public string DisplayName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string? AvatarUrl { get; init; }
        public bool IsOnline { get; init; }
        public DateTime? LastSeen { get; init; }
        public string? PublicKey { get; init; }
    }
}
