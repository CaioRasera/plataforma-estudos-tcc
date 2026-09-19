using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudyPlatform.Domain.Entities;
using StudyPlatform.Domain.Interfaces.Repositories;

namespace StudyPlatform.Infrastructure.Persistence.Repositories;

public class ProgressRepository : IProgressRepository
{
    private readonly AppDbContext _context;

    public ProgressRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddRangeAsync(IEnumerable<UserItemProgress> progresses)
    {
        _context.UserItemProgresses.AddRange(progresses);
        await _context.SaveChangesAsync();
    }

    public async Task<List<UserItemProgress>> GetAllByUserIdAsync(Guid userId)
    {
        return await _context.UserItemProgresses
            .Include(p => p.Item)
            .Where(p => p.UserId == userId)
            .ToListAsync();
    }

    public async Task<UserItemProgress?> GetByIdAsync(Guid id)
    {
        return await _context.UserItemProgresses
            .Include(p => p.Item)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<UserItemProgress>> GetDueForReviewAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        return await _context.UserItemProgresses
            .Include(p => p.Item)
            .Where(p => p.UserId == userId && p.NextReview <= now)
            .OrderBy(p => p.NextReview)
            .ToListAsync();
    }

    public async Task UpdateAsync(UserItemProgress progress)
    {
        _context.UserItemProgresses.Update(progress);
        await _context.SaveChangesAsync();
    }
}
