using System;
using System.Collections.Generic;

namespace StudyPlatform.Domain.Entities;

public class AssessmentItem
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Type { get; set; } = string.Empty; // "Flashcard" or "Quiz"
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? Options { get; set; } // JSON array
    public string? Topic { get; set; }
    public Guid[]? SourceChunkIds { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
