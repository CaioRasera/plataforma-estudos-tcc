using System;

namespace StudyPlatform.Domain.Entities;

public class UserItemProgress
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid ItemId { get; set; }
    public AssessmentItem Item { get; set; } = null!;
    
    public float EasinessFactor { get; set; } = 2.5f;
    public int Interval { get; set; } = 0;
    public int Repetitions { get; set; } = 0;
    public DateTime NextReview { get; set; } = DateTime.UtcNow;
    public DateTime? LastReviewed { get; set; }
    public int? LastQuality { get; set; }
}
