using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudyPlatform.Domain.Entities;
using StudyPlatform.Domain.Interfaces.Repositories;

namespace StudyPlatform.Infrastructure.Persistence.Repositories;

public class AssessmentRepository : IAssessmentRepository
{
    private readonly AppDbContext _context;

    public AssessmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddRangeAsync(IEnumerable<AssessmentItem> items)
    {
        _context.AssessmentItems.AddRange(items);
        await _context.SaveChangesAsync();
    }

    public async Task<List<AssessmentItem>> GetByUserIdAsync(Guid userId, Guid? documentId = null, string? type = null)
    {
        var query = _context.AssessmentItems.Where(a => a.UserId == userId);
        if (documentId.HasValue) query = query.Where(a => a.DocumentId == documentId.Value);
        if (!string.IsNullOrEmpty(type)) query = query.Where(a => a.Type == type);

        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    public async Task<List<AssessmentItem>> GetByDocumentIdAsync(Guid documentId)
    {
        return await _context.AssessmentItems
            .Where(a => a.DocumentId == documentId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }
}
