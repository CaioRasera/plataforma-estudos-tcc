using System.Threading;
using System.Threading.Tasks;
using StudyPlatform.Domain.Interfaces;

namespace StudyPlatform.Infrastructure.AiProviders;

/// <summary>
/// Routes AI calls to the appropriate provider based on the operation type.
/// Text generation uses Groq; embedding uses Gemini.
/// </summary>
public class HybridAiProvider : IAiProvider
{
    private readonly GroqProvider _groq;
    private readonly GeminiProvider _gemini;

    public HybridAiProvider(GroqProvider groq, GeminiProvider gemini)
    {
        _groq = groq;
        _gemini = gemini;
    }

    public Task<string> GenerateAsync(string prompt, string? context = null, string itemType = "Flashcard", CancellationToken cancellationToken = default)
    {
        return _groq.GenerateAsync(prompt, context, itemType, cancellationToken);
    }

    public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        return _gemini.EmbedAsync(text, cancellationToken);
    }
}
