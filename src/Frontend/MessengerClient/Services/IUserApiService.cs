using Refit;
using MessengerContracts.DTOs;

namespace MessengerClient.Services
{
    public interface IUserApiService
    {
        [Get("/api/users/profile")]
        Task<UserDto> GetProfileAsync([Header("Authorization")] string authorization);

        [Put("/api/users/profile")]
        Task<UserDto> UpdateProfileAsync([Header("Authorization")] string authorization, [Body] UpdateProfileRequest request);

        [Get("/api/users/contacts")]
        Task<List<ContactDto>> GetContactsAsync([Header("Authorization")] string authorization);

        [Post("/api/users/contacts")]
        Task<ContactDto> AddContactAsync([Header("Authorization")] string authorization, [Body] AddContactRequest request);

        [Delete("/api/users/contacts/{contactId}")]
        Task DeleteContactAsync([Header("Authorization")] string authorization, Guid contactId);

        [Get("/api/users/search")]
        Task<List<UserDto>> SearchUsersAsync([Header("Authorization")] string authorization, [Query] string query);
    }
}
