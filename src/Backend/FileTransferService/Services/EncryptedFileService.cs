using FileTransferService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace FileTransferService.Services
{
    public interface IEncryptedFileService
    {
        Task<Guid> UploadFileAsync(byte[] encryptedContent, string fileName, long fileSize, Guid senderId, Guid recipientId);
        Task<byte[]> DownloadFileAsync(Guid fileId, Guid userId);
        Task<bool> DeleteFileAsync(Guid fileId, Guid userId);
    }

    public class EncryptedFileService : IEncryptedFileService
    {
        private readonly FileDbContext _context;
        private readonly ILogger<EncryptedFileService> _logger;

        public EncryptedFileService(FileDbContext context, ILogger<EncryptedFileService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Guid> UploadFileAsync(byte[] encryptedContent, string fileName, long fileSize, Guid senderId, Guid recipientId)
        {
            var fileMetadata = new FileTransferService.Data.FileMetadata
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                RecipientId = recipientId,
                OriginalFilename = fileName,
                FileSizeBytes = fileSize,
                ContentType = "application/octet-stream",
                StoragePath = $"/files/{Guid.NewGuid()}",
                EncryptedKey = new byte[32], // Placeholder
                Nonce = new byte[12],
                UploadTimestamp = DateTime.UtcNow,
                DownloadCount = 0,
                IsDeleted = false
            };

            await _context.FileMetadata.AddAsync(fileMetadata);
            await _context.SaveChangesAsync();

            _logger.LogInformation("File uploaded: {FileId}", fileMetadata.Id);

            return fileMetadata.Id;
        }

        public async Task<byte[]> DownloadFileAsync(Guid fileId, Guid userId)
        {
            var file = await _context.FileMetadata
                .FirstOrDefaultAsync(f => f.Id == fileId && !f.IsDeleted);

            if (file == null || (file.SenderId != userId && file.RecipientId != userId))
            {
                throw new UnauthorizedAccessException();
            }

            file.DownloadCount++;
            await _context.SaveChangesAsync();

            return new byte[0]; // Placeholder
        }

        public async Task<bool> DeleteFileAsync(Guid fileId, Guid userId)
        {
            var file = await _context.FileMetadata
                .FirstOrDefaultAsync(f => f.Id == fileId && !f.IsDeleted);

            if (file == null || (file.SenderId != userId && file.RecipientId != userId))
            {
                return false;
            }

            file.IsDeleted = true;
            file.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
