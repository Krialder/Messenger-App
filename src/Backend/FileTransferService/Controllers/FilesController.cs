using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MessengerContracts.DTOs;
using FileTransferService.Services;
using MessengerCommon.Constants;

namespace FileTransferService.Controllers
{
    /// <summary>
    /// Controller for encrypted file upload/download
    /// UC-012: File Upload with Encryption
    /// </summary>
    [ApiController]
    [Route("api/files")]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly IEncryptedFileService _fileService;
        private readonly ILogger<FilesController> _logger;

        public FilesController(IEncryptedFileService fileService, ILogger<FilesController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Upload encrypted file
        /// POST /api/files/upload
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromBody] FileUploadDto fileUpload)
        {
            try
            {
                // PSEUDO: Validate file size
                if (fileUpload.FileSize > ConfigConstants.MAX_FILE_SIZE_BYTES)
                {
                    return BadRequest(new { error = "File size exceeds maximum limit (100 MB)" });
                }

                // PSEUDO: Get current user ID from JWT claims
                var userId = Guid.Parse(User.Identity?.Name ?? throw new UnauthorizedAccessException());

                // PSEUDO: Store encrypted file
                var storedFile = await _fileService.StoreEncryptedFileAsync(fileUpload, userId);

                _logger.LogInformation("File {FileId} uploaded by user {UserId}", storedFile.Id, userId);

                return Ok(new
                {
                    fileId = storedFile.Id,
                    fileName = storedFile.FileName,
                    uploadedAt = storedFile.UploadedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(500, new { error = "File upload failed" });
            }
        }

        /// <summary>
        /// Download encrypted file
        /// GET /api/files/{fileId}
        /// </summary>
        [HttpGet("{fileId}")]
        public async Task<IActionResult> DownloadFile(Guid fileId)
        {
            try
            {
                // PSEUDO: Get current user ID
                var userId = Guid.Parse(User.Identity?.Name ?? throw new UnauthorizedAccessException());

                // PSEUDO: Retrieve encrypted file
                var file = await _fileService.RetrieveEncryptedFileAsync(fileId, userId);

                if (file == null)
                {
                    return NotFound(new { error = "File not found or access denied" });
                }

                _logger.LogInformation("File {FileId} downloaded by user {UserId}", fileId, userId);

                return Ok(file);
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

        /// <summary>
        /// Delete file
        /// DELETE /api/files/{fileId}
        /// </summary>
        [HttpDelete("{fileId}")]
        public async Task<IActionResult> DeleteFile(Guid fileId)
        {
            try
            {
                // PSEUDO: Get current user ID
                var userId = Guid.Parse(User.Identity?.Name ?? throw new UnauthorizedAccessException());

                // PSEUDO: Delete file (only if user is sender or recipient)
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

        /// <summary>
        /// Get user's uploaded files
        /// GET /api/files/my-files
        /// </summary>
        [HttpGet("my-files")]
        public async Task<IActionResult> GetMyFiles([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                // PSEUDO: Get current user ID
                var userId = Guid.Parse(User.Identity?.Name ?? throw new UnauthorizedAccessException());

                // PSEUDO: Retrieve user's files
                var files = await _fileService.GetUserFilesAsync(userId, page, pageSize);

                return Ok(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving files for user");
                return StatusCode(500, new { error = "Failed to retrieve files" });
            }
        }
    }
}
