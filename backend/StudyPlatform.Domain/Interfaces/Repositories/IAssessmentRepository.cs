using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudyPlatform.Domain.Entities;

namespace StudyPlatform.Domain.Interfaces.Repositories;

public interface IAssessmentRepository
{
    Task AddRangeAsync(IEnumerable<AssessmentItem> items);
    Task<List<AssessmentItem>> GetByUserIdAsync(Guid userId, Guid? documentId = null, string? type = null);
    Task<List<AssessmentItem>> GetByDocumentIdAsync(Guid documentId);
}
