using System;
using System.IO;
using System.Threading.Tasks;
using Refit;
using MessengerContracts.DTOs;

namespace MessengerClient.Services
{
    public interface IFileApiService
    {
        [Multipart]
        [Post("/api/files")]
        Task<FileUploadResponse> UploadFileAsync([AliasAs("file")] StreamPart file, [Header("Authorization")] string authorization);

        [Get("/api/files/{fileId}")]
        Task<FileDownloadResponse> DownloadFileAsync(Guid fileId, [Header("Authorization")] string authorization);

        [Delete("/api/files/{fileId}")]
        Task DeleteFileAsync(Guid fileId, [Header("Authorization")] string authorization);

        [Get("/api/files/{fileId}/metadata")]
        Task<FileMetadataDto> GetFileMetadataAsync(Guid fileId, [Header("Authorization")] string authorization);
    }

    // Supporting DTOs
    public record FileUploadResponse(
        Guid FileId,
        string FileName,
        long Size,
        string ContentType,
        string EncryptedUrl
    );

    public record FileDownloadResponse(
        string FileName,
        byte[] EncryptedContent,
        string ContentType
    );

    public record FileMetadataDto(
        Guid Id,
        string FileName,
        long Size,
        string ContentType,
        DateTime UploadedAt,
        Guid UploadedBy
    );
}
