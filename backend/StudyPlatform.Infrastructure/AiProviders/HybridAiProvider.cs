using System.Threading;
using System.Threading.Tasks;
using StudyPlatform.Domain.Interfaces;

namespace StudyPlatform.Infrastructure.AiProviders;

public class HybridAiProvider : IAiProvider
{
    private readonly GroqProvider _groq;
    private readonly GeminiProvider _gemini;

    public HybridAiProvider(GroqProvider groq, GeminiProvider gemini)
    {
        _groq = groq;
        _gemini = gemini;
    }

    public Task<string> GenerateAsync(string prompt, string? context = null, CancellationToken cancellationToken = default)
    {
        // Usa a Groq para gerar os textos velozmente sem limite de 20 requests/dia
        return _groq.GenerateAsync(prompt, context, cancellationToken);
    }

    public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        // Mantém o Gemini apenas para gerar os vetores matemáticos
        return _gemini.EmbedAsync(text, cancellationToken);
    }
}