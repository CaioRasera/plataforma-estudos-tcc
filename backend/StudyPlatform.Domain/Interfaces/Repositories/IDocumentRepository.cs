using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudyPlatform.Domain.Entities;

namespace StudyPlatform.Domain.Interfaces.Repositories;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid id);
    Task<List<Document>> GetByUserIdAsync(Guid userId);
    Task AddAsync(Document document);
    Task UpdateAsync(Document document);
    Task DeleteAsync(Document document);
}
