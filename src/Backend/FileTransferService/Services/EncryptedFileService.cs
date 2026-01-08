using MessengerContracts.DTOs;
using FileTransferService.Data;
using Microsoft.EntityFrameworkCore;

namespace FileTransferService.Services
{
    /// <summary>
    /// Service for encrypted file storage and retrieval
    /// Files are already encrypted client-side before upload
    /// </summary>
    public class EncryptedFileService : IEncryptedFileService
    {
        private readonly FileDbContext _context;
        private readonly ILogger<EncryptedFileService> _logger;

        public EncryptedFileService(FileDbContext context, ILogger<EncryptedFileService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<FileUploadDto> StoreEncryptedFileAsync(FileUploadDto fileUpload, Guid currentUserId)
        {
            // PSEUDO: Validate that current user is the sender
            if (fileUpload.SenderId != currentUserId)
            {
                throw new UnauthorizedAccessException("Sender ID mismatch");
            }

            // PSEUDO: Create file entity
            var fileEntity = new EncryptedFile
            {
                Id = Guid.NewGuid(),
                FileName = fileUpload.FileName,
                FileSize = fileUpload.FileSize,
                EncryptedContent = fileUpload.EncryptedContent,
                EncryptionMetadata = fileUpload.EncryptionMetadata,
                SenderId = fileUpload.SenderId,
                RecipientId = fileUpload.RecipientId,
                UploadedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            // PSEUDO: Save to database
            _context.EncryptedFiles.Add(fileEntity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Stored encrypted file {FileId}", fileEntity.Id);

            return new FileUploadDto
            {
                Id = fileEntity.Id,
                FileName = fileEntity.FileName,
                FileSize = fileEntity.FileSize,
                UploadedAt = fileEntity.UploadedAt
            };
        }

        public async Task<FileDownloadDto?> RetrieveEncryptedFileAsync(Guid fileId, Guid currentUserId)
        {
            // PSEUDO: Retrieve file
            var file = await _context.EncryptedFiles
                .FirstOrDefaultAsync(f => f.Id == fileId && !f.IsDeleted);

            if (file == null)
            {
                return null;
            }

            // PSEUDO: Check authorization (user must be sender or recipient)
            if (file.SenderId != currentUserId && file.RecipientId != currentUserId)
            {
                throw new UnauthorizedAccessException("Access denied");
            }

            return new FileDownloadDto
            {
                FileId = file.Id,
                FileName = file.FileName,
                FileSize = file.FileSize,
                EncryptedContent = file.EncryptedContent
            };
        }

        public async Task<bool> DeleteFileAsync(Guid fileId, Guid currentUserId)
        {
            // PSEUDO: Find file
            var file = await _context.EncryptedFiles
                .FirstOrDefaultAsync(f => f.Id == fileId && !f.IsDeleted);

            if (file == null)
            {
                return false;
            }

            // PSEUDO: Check authorization
            if (file.SenderId != currentUserId && file.RecipientId != currentUserId)
            {
                throw new UnauthorizedAccessException("Access denied");
            }

            // PSEUDO: Soft delete
            file.IsDeleted = true;
            file.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("File {FileId} soft deleted", fileId);

            return true;
        }

        public async Task<List<FileUploadDto>> GetUserFilesAsync(Guid userId, int page, int pageSize)
        {
            // PSEUDO: Retrieve files where user is sender or recipient
            var files = await _context.EncryptedFiles
                .Where(f => !f.IsDeleted && (f.SenderId == userId || f.RecipientId == userId))
                .OrderByDescending(f => f.UploadedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FileUploadDto
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    FileSize = f.FileSize,
                    SenderId = f.SenderId,
                    RecipientId = f.RecipientId,
                    UploadedAt = f.UploadedAt
                })
                .ToListAsync();

            return files;
        }
    }

    /// <summary>
    /// Interface for encrypted file service
    /// </summary>
    public interface IEncryptedFileService
    {
        Task<FileUploadDto> StoreEncryptedFileAsync(FileUploadDto fileUpload, Guid currentUserId);
        Task<FileDownloadDto?> RetrieveEncryptedFileAsync(Guid fileId, Guid currentUserId);
        Task<bool> DeleteFileAsync(Guid fileId, Guid currentUserId);
        Task<List<FileUploadDto>> GetUserFilesAsync(Guid userId, int page, int pageSize);
    }
}
