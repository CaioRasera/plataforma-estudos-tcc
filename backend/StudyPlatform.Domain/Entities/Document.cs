using System;
using System.Collections.Generic;

namespace StudyPlatform.Domain.Entities;

public class Document
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public int? TotalChunks { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    
    public ICollection<Chunk> Chunks { get; set; } = new List<Chunk>();
    public ICollection<AssessmentItem> AssessmentItems { get; set; } = new List<AssessmentItem>();
}
