using System;

namespace MessengerContracts.DTOs
{
    /// <summary>
    /// DTO for encrypted file upload
    /// </summary>
    public class FileUploadDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string EncryptedContent { get; set; } = string.Empty;
        public string EncryptionMetadata { get; set; } = string.Empty;
        public Guid SenderId { get; set; }
        public Guid RecipientId { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    /// <summary>
    /// DTO for file download request
    /// </summary>
    public class FileDownloadDto
    {
        public Guid FileId { get; set; }
        public string EncryptedContent { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }
}
