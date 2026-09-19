using System.Collections.Generic;

namespace StudyPlatform.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int TotalItems { get; set; }
    public int ReviewedItems { get; set; }
    public int DueToday { get; set; }
    public int StreakDays { get; set; }
    public int TotalDocuments { get; set; }
    public List<DailyReviewDto> ReviewsLast7Days { get; set; } = new();
    public List<DocumentProgressDto> DocumentProgress { get; set; } = new();
}

public class DailyReviewDto
{
    public string Date { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DocumentProgressDto
{
    public string DocumentTitle { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public int ReviewedItems { get; set; }
}