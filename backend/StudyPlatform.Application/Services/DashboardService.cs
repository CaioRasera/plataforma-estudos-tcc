using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudyPlatform.Application.DTOs.Dashboard;
using StudyPlatform.Domain.Interfaces.Repositories;

namespace StudyPlatform.Application.Services;

public class DashboardService
{
    private readonly IProgressRepository _progressRepository;
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IDocumentRepository _documentRepository;

    public DashboardService(
        IProgressRepository progressRepository,
        IAssessmentRepository assessmentRepository,
        IDocumentRepository documentRepository)
    {
        _progressRepository = progressRepository;
        _assessmentRepository = assessmentRepository;
        _documentRepository = documentRepository;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(Guid userId)
    {
        var allProgress = await _progressRepository.GetAllByUserIdAsync(userId);
        var allItems = await _assessmentRepository.GetByUserIdAsync(userId);
        var documents = await _documentRepository.GetByUserIdAsync(userId);

        var now = DateTime.UtcNow;
        var dueToday = 0;
        var reviewed = 0;
        var streakDays = 0;

        var reviewsByDay = new Dictionary<string, int>();

        foreach (var p in allProgress)
        {
            if (p.NextReview <= now) dueToday++;
            if (p.LastReviewed.HasValue)
            {
                reviewed++;
                var dayKey = p.LastReviewed.Value.ToString("yyyy-MM-dd");
                if (!reviewsByDay.ContainsKey(dayKey)) reviewsByDay[dayKey] = 0;
                reviewsByDay[dayKey]++;
            }
        }

        // Calculate streak
        var date = now.Date;
        while (reviewsByDay.ContainsKey(date.ToString("yyyy-MM-dd")))
        {
            streakDays++;
            date = date.AddDays(-1);
        }

        // Build chart data (last 7 days)
        var chartData = new List<DailyReviewDto>();
        for (int i = 6; i >= 0; i--)
        {
            var d = now.Date.AddDays(-i);
            var key = d.ToString("yyyy-MM-dd");
            chartData.Add(new DailyReviewDto
            {
                Date = d.ToString("dd/MM"),
                Count = reviewsByDay.ContainsKey(key) ? reviewsByDay[key] : 0
            });
        }

        // Per-document progress
        var docProgress = new List<DocumentProgressDto>();
        foreach (var doc in documents)
        {
            var docItems = await _assessmentRepository.GetByDocumentIdAsync(doc.Id);
            var docReviewed = 0;
            foreach (var item in docItems)
            {
                foreach (var prog in allProgress)
                {
                    if (prog.ItemId == item.Id && prog.Repetitions > 0) { docReviewed++; break; }
                }
            }
            docProgress.Add(new DocumentProgressDto
            {
                DocumentTitle = doc.Title,
                TotalItems = docItems.Count,
                ReviewedItems = docReviewed
            });
        }

        return new DashboardSummaryDto
        {
            TotalItems = allItems.Count,
            ReviewedItems = reviewed,
            DueToday = dueToday,
            StreakDays = streakDays,
            TotalDocuments = documents.Count,
            ReviewsLast7Days = chartData,
            DocumentProgress = docProgress
        };
    }
}