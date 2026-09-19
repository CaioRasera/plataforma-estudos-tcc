using System.Threading;
using System.Threading.Tasks;

namespace StudyPlatform.Domain.Interfaces;

/// <summary>
/// Abstração genérica para provedores de IA generativa.
/// </summary>
public interface IAiProvider
{
    Task<string> GenerateAsync(string prompt, string? context = null, CancellationToken cancellationToken = default);
    Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default);
}
