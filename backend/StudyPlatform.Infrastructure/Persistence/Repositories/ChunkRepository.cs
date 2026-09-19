using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using StudyPlatform.Domain.Entities;
using StudyPlatform.Domain.Interfaces.Repositories;
using Pgvector;

namespace StudyPlatform.Infrastructure.Persistence.Repositories;

public class ChunkRepository : IChunkRepository
{
    private readonly AppDbContext _context;

    public ChunkRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddRangeAsync(IEnumerable<Chunk> chunks)
    {
        _context.Chunks.AddRange(chunks);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Chunk>> FindSimilarAsync(float[] queryEmbedding, Guid documentId, int topK = 5)
    {
        var vector = new Vector(queryEmbedding);
        return await _context.Chunks
            .Where(c => c.DocumentId == documentId && c.Embedding != null)
            .OrderBy(c => c.Embedding!.CosineDistance(vector))
            .Take(topK)
            .ToListAsync();
    }
}
