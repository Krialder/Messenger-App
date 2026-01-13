using Refit;
using MessengerContracts.DTOs;

namespace MessengerClient.Services
{
    public interface IFileApiService
    {
        [Multipart]
        [Post("/api/files/upload")]
        Task<FileUploadResponse> UploadFileAsync([Header("Authorization")] string authorization, [AliasAs("file")] StreamPart file);

        [Get("/api/files/{fileId}")]
        Task<FileDownloadResponse> DownloadFileAsync([Header("Authorization")] string authorization, Guid fileId);

        [Delete("/api/files/{fileId}")]
        Task DeleteFileAsync([Header("Authorization")] string authorization, Guid fileId);

        [Get("/api/files/{fileId}/metadata")]
        Task<FileMetadataDto> GetFileMetadataAsync([Header("Authorization")] string authorization, Guid fileId);
    }
}
