using System.Threading;
using System.Threading.Tasks;

namespace StudyPlatform.Domain.Interfaces;

public interface IAiProvider
{
    Task<string> GenerateAsync(string prompt, string? context = null, string itemType = "Flashcard", CancellationToken cancellationToken = default);
    Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default);
}
