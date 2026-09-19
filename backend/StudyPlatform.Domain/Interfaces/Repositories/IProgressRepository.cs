using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudyPlatform.Domain.Entities;

namespace StudyPlatform.Domain.Interfaces.Repositories;

public interface IProgressRepository
{
    Task<UserItemProgress?> GetByIdAsync(Guid id);
    Task<List<UserItemProgress>> GetDueForReviewAsync(Guid userId);
    Task AddRangeAsync(IEnumerable<UserItemProgress> progresses);
    Task UpdateAsync(UserItemProgress progress);
    Task<List<UserItemProgress>> GetAllByUserIdAsync(Guid userId);
}
