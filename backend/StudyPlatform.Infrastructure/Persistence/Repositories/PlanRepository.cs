using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudyPlatform.Domain.Entities;
using StudyPlatform.Domain.Interfaces.Repositories;

namespace StudyPlatform.Infrastructure.Persistence.Repositories;

public class PlanRepository : IPlanRepository
{
    private readonly AppDbContext _context;

    public PlanRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Plan?> GetByIdAsync(int id)
    {
        return await _context.Plans.FindAsync(id);
    }

    public async Task<List<Plan>> GetAllAsync()
    {
        return await _context.Plans.ToListAsync();
    }
}