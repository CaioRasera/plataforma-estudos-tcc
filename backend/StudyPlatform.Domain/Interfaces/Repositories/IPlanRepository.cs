using System.Collections.Generic;
using System.Threading.Tasks;
using StudyPlatform.Domain.Entities;

namespace StudyPlatform.Domain.Interfaces.Repositories;

public interface IPlanRepository
{
    Task<Plan?> GetByIdAsync(int id);
    Task<List<Plan>> GetAllAsync();
}