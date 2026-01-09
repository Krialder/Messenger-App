using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FileTransferService.Data;
using FileTransferService.Services;

namespace MessengerTests.ServiceTests
{
    public class FileTransferServiceTests : IDisposable
    {
        private readonly FileDbContext _context;
        private readonly EncryptedFileService _service;
        private readonly Mock<ILogger<EncryptedFileService>> _mockLogger;

        public FileTransferServiceTests()
        {
            var options = new DbContextOptionsBuilder<FileDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new FileDbContext(options);

            _mockLogger = new Mock<ILogger<EncryptedFileService>>();

            _service = new EncryptedFileService(_context, _mockLogger.Object);
        }

        // ========================================
        // Upload Tests
        // ========================================

        [Fact]
        public async Task UploadFile_ValidFile_Success()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            var recipientId = Guid.NewGuid();
            var fileName = "test-document.pdf";
            var fileContent = new byte[] { 0x1, 0x2, 0x3, 0x4 };
            var fileSize = fileContent.Length;

            // Act
            var result = await _service.UploadFileAsync(fileContent, fileName, fileSize, senderId, recipientId);

            // Assert
            Assert.NotEqual(Guid.Empty, result);

            var savedMetadata = await _context.FileMetadata.FindAsync(result);
            Assert.NotNull(savedMetadata);
            Assert.Equal(senderId, savedMetadata.SenderId);
            Assert.Equal(recipientId, savedMetadata.RecipientId);
            Assert.Equal(fileName, savedMetadata.OriginalFilename);
        }

        [Fact]
        public async Task UploadFile_StoresMetadata()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            var recipientId = Guid.NewGuid();
            var fileName = "image.jpg";
            var fileContent = new byte[1024];
            
            // Act
            var fileId = await _service.UploadFileAsync(fileContent, fileName, fileContent.Length, senderId, recipientId);

            // Assert
            var metadata = await _context.FileMetadata.FindAsync(fileId);
            Assert.NotNull(metadata);
            Assert.Equal(fileContent.Length, metadata.FileSizeBytes);
            Assert.False(metadata.IsDeleted);
        }

        // ========================================
        // Download Tests
        // ========================================

        [Fact]
        public async Task DownloadFile_AsRecipient_Success()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            var recipientId = Guid.NewGuid();
            var fileContent = new byte[] { 0x1, 0x2, 0x3 };

            var fileId = await _service.UploadFileAsync(fileContent, "test.txt", fileContent.Length, senderId, recipientId);

            // Act
            var downloadResult = await _service.DownloadFileAsync(fileId, recipientId);

            // Assert
            Assert.NotNull(downloadResult);
        }

        [Fact]
        public async Task DownloadFile_AsSender_Success()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            var recipientId = Guid.NewGuid();
            var fileContent = new byte[] { 0x1, 0x2 };

            var fileId = await _service.UploadFileAsync(fileContent, "test.txt", fileContent.Length, senderId, recipientId);

            // Act
            var downloadResult = await _service.DownloadFileAsync(fileId, senderId);

            // Assert
            Assert.NotNull(downloadResult);
        }

        [Fact]
        public async Task DownloadFile_Unauthorized_ThrowsException()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            var recipientId = Guid.NewGuid();
            var unauthorizedUserId = Guid.NewGuid();
            var fileContent = new byte[] { 0x1 };

            var fileId = await _service.UploadFileAsync(fileContent, "test.txt", fileContent.Length, senderId, recipientId);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _service.DownloadFileAsync(fileId, unauthorizedUserId)
            );
        }

        [Fact]
        public async Task DownloadFile_IncrementsDownloadCount()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            var recipientId = Guid.NewGuid();
            var fileContent = new byte[] { 0x1 };

            var fileId = await _service.UploadFileAsync(fileContent, "test.txt", fileContent.Length, senderId, recipientId);

            // Act
            await _service.DownloadFileAsync(fileId, recipientId);
            await _service.DownloadFileAsync(fileId, recipientId);

            // Assert
            var metadata = await _context.FileMetadata.FindAsync(fileId);
            Assert.Equal(2, metadata!.DownloadCount);
        }

        // ========================================
        // Delete Tests
        // ========================================

        [Fact]
        public async Task DeleteFile_AsOwner_Success()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            var recipientId = Guid.NewGuid();
            var fileContent = new byte[] { 0x1 };

            var fileId = await _service.UploadFileAsync(fileContent, "test.txt", fileContent.Length, senderId, recipientId);

            // Act
            var deleted = await _service.DeleteFileAsync(fileId, senderId);

            // Assert
            Assert.True(deleted);
            var metadata = await _context.FileMetadata.FindAsync(fileId);
            Assert.True(metadata!.IsDeleted);
            Assert.NotNull(metadata.DeletedAt);
        }

        [Fact]
        public async Task DeleteFile_AsRecipient_Success()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            var recipientId = Guid.NewGuid();
            var fileContent = new byte[] { 0x1 };

            var fileId = await _service.UploadFileAsync(fileContent, "test.txt", fileContent.Length, senderId, recipientId);

            // Act
            var deleted = await _service.DeleteFileAsync(fileId, recipientId);

            // Assert
            Assert.True(deleted);
        }

        [Fact]
        public async Task DeleteFile_Unauthorized_ReturnsFalse()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            var recipientId = Guid.NewGuid();
            var unauthorizedUserId = Guid.NewGuid();
            var fileContent = new byte[] { 0x1 };

            var fileId = await _service.UploadFileAsync(fileContent, "test.txt", fileContent.Length, senderId, recipientId);

            // Act
            var deleted = await _service.DeleteFileAsync(fileId, unauthorizedUserId);

            // Assert
            Assert.False(deleted);
        }

        [Fact]
        public async Task DeleteFile_FileNotFound_ReturnsFalse()
        {
            // Arrange
            var nonExistentFileId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act
            var deleted = await _service.DeleteFileAsync(nonExistentFileId, userId);

            // Assert
            Assert.False(deleted);
        }

        [Fact]
        public async Task DeleteFile_SoftDelete_DoesNotRemoveFromDatabase()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            var recipientId = Guid.NewGuid();
            var fileContent = new byte[] { 0x1 };

            var fileId = await _service.UploadFileAsync(fileContent, "test.txt", fileContent.Length, senderId, recipientId);

            // Act
            await _service.DeleteFileAsync(fileId, senderId);

            // Assert
            var metadata = await _context.FileMetadata.FindAsync(fileId);
            Assert.NotNull(metadata);
            Assert.True(metadata.IsDeleted);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
