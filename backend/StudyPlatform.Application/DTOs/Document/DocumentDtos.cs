using System;

namespace StudyPlatform.Application.DTOs.Document;

public class DocumentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UploadResponseDto
{
    public Guid DocumentId { get; set; }
    public string Message { get; set; } = string.Empty;
}
