using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudyPlatform.Domain.Entities;

namespace StudyPlatform.Domain.Interfaces.Repositories;

public interface IChunkRepository
{
    Task AddRangeAsync(IEnumerable<Chunk> chunks);
    Task<List<Chunk>> FindSimilarAsync(float[] queryEmbedding, Guid documentId, int topK = 5);
}
