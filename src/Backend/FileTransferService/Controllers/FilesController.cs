using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FileTransferService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly Services.IEncryptedFileService _fileService;
        private readonly ILogger<FilesController> _logger;

        public FilesController(Services.IEncryptedFileService fileService, ILogger<FilesController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromBody] UploadFileRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                {
                    return Unauthorized();
                }

                var fileId = await _fileService.UploadFileAsync(
                    request.EncryptedContent,
                    request.FileName,
                    request.FileSize,
                    userId,
                    request.RecipientId);

                _logger.LogInformation("File {FileId} uploaded by user {UserId}", fileId, userId);

                return Ok(new { FileId = fileId, FileName = request.FileName, UploadedAt = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(500, new { error = "File upload failed" });
            }
        }

        [HttpGet("{fileId}")]
        public async Task<IActionResult> DownloadFile(Guid fileId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                {
                    return Unauthorized();
                }

                var fileContent = await _fileService.DownloadFileAsync(fileId, userId);

                _logger.LogInformation("File {FileId} downloaded by user {UserId}", fileId, userId);

                return File(fileContent, "application/octet-stream");
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file {FileId}", fileId);
                return StatusCode(500, new { error = "File download failed" });
            }
        }

        [HttpDelete("{fileId}")]
        public async Task<IActionResult> DeleteFile(Guid fileId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                {
                    return Unauthorized();
                }

                var deleted = await _fileService.DeleteFileAsync(fileId, userId);

                if (!deleted)
                {
                    return NotFound(new { error = "File not found or access denied" });
                }

                _logger.LogInformation("File {FileId} deleted by user {UserId}", fileId, userId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileId}", fileId);
                return StatusCode(500, new { error = "File deletion failed" });
            }
        }
    }

    public class UploadFileRequest
    {
        public byte[] EncryptedContent { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public Guid RecipientId { get; set; }
    }
}
