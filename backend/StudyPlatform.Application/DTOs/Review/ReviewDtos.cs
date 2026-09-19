using System;
using System.Collections.Generic;

namespace StudyPlatform.Application.DTOs.Review;

public class ReviewItemDto
{
    public Guid ProgressId { get; set; }
    public Guid ItemId { get; set; }
    public Guid DocumentId { get; set; }
    public string DocumentTitle { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? Options { get; set; }
}

public class SubmitReviewDto
{
    public Guid ProgressId { get; set; }
    public int Quality { get; set; } // 0-5
}