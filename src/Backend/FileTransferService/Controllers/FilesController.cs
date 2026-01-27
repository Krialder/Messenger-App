using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FileTransferService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class FilesController : ControllerBase
    {
        private readonly Services.IEncryptedFileService _fileService;
        private readonly ILogger<FilesController> _logger;
        private const long MaxFileSize = 100 * 1024 * 1024; // 100 MB

        public FilesController(Services.IEncryptedFileService fileService, ILogger<FilesController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Upload an encrypted file
        /// </summary>
        [HttpPost("upload")]
        [ProducesResponseType(typeof(UploadFileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadFile([FromBody] UploadFileRequest request)
        {
            try
            {
                // Validation
                if (request.EncryptedContent == null || request.EncryptedContent.Length == 0)
                {
                    return BadRequest(new { error = "File content is required" });
                }

                if (string.IsNullOrWhiteSpace(request.FileName))
                {
                    return BadRequest(new { error = "File name is required" });
                }

                if (request.FileSize <= 0 || request.FileSize > MaxFileSize)
                {
                    return StatusCode(413, new { error = $"File size must be between 1 byte and {MaxFileSize / (1024 * 1024)} MB" });
                }

                var userId = GetUserId();

                var fileId = await _fileService.UploadFileAsync(
                    request.EncryptedContent,
                    request.FileName,
                    request.FileSize,
                    userId,
                    request.RecipientId);

                _logger.LogInformation("File {FileId} ({FileName}, {FileSize} bytes) uploaded by user {UserId} for recipient {RecipientId}", 
                    fileId, request.FileName, request.FileSize, userId, request.RecipientId);

                return Ok(new UploadFileResponse
                {
                    FileId = fileId,
                    FileName = request.FileName,
                    FileSize = request.FileSize,
                    UploadedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FileName}", request?.FileName);
                return StatusCode(500, new { error = "File upload failed" });
            }
        }

        /// <summary>
        /// Download an encrypted file
        /// </summary>
        [HttpGet("{fileId}")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadFile(Guid fileId)
        {
            try
            {
                var userId = GetUserId();

                var fileContent = await _fileService.DownloadFileAsync(fileId, userId);

                if (fileContent == null || fileContent.Length == 0)
                {
                    return NotFound(new { error = "File not found" });
                }

                _logger.LogInformation("File {FileId} downloaded by user {UserId}", fileId, userId);

                return File(fileContent, "application/octet-stream");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt to file {FileId} by user {UserId}", 
                    fileId, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return Forbid();
            }
            catch (FileNotFoundException)
            {
                return NotFound(new { error = "File not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file {FileId}", fileId);
                return StatusCode(500, new { error = "File download failed" });
            }
        }

        /// <summary>
        /// Delete a file
        /// </summary>
        [HttpDelete("{fileId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteFile(Guid fileId)
        {
            try
            {
                var userId = GetUserId();

                var deleted = await _fileService.DeleteFileAsync(fileId, userId);

                if (!deleted)
                {
                    return NotFound(new { error = "File not found or access denied" });
                }

                _logger.LogInformation("File {FileId} deleted by user {UserId}", fileId, userId);

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileId}", fileId);
                return StatusCode(500, new { error = "File deletion failed" });
            }
        }

        #region Helper Methods

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                _logger.LogError("Invalid or missing user ID claim");
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            return userId;
        }

        #endregion
    }

    #region DTOs

    public class UploadFileRequest
    {
        public byte[] EncryptedContent { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public Guid RecipientId { get; set; }
    }

    public class UploadFileResponse
    {
        public Guid FileId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    #endregion
}
