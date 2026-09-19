using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StudyPlatform.Domain.Interfaces;

namespace StudyPlatform.Infrastructure.AiProviders;

public class GroqProvider : IAiProvider
{
    private static readonly HttpClient _http = new();
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _systemPromptTemplate;

    public GroqProvider(IConfiguration config)
    {
        _apiKey = config["AiProvider:Groq:ApiKey"]
            ?? throw new InvalidOperationException("AiProvider:Groq:ApiKey not configured.");
        _model = "openai/gpt-oss-120b";
        _systemPromptTemplate = LoadPrompt("flashcard-system.txt");
    }

    public async Task<string> GenerateAsync(string prompt, string? context = null, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var systemPrompt = _systemPromptTemplate.Replace("{prompt}", prompt);

            var userContent = context is { Length: > 0 }
                ? $"TEXTO PARA ANALISE:\n{context}"
                : prompt;

            var body = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user",   content = userContent  }
                },
                temperature = 0.2,
                max_tokens = 4000
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            using var response = await _http.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Groq error ({response.StatusCode}): {errorContent}", null, response.StatusCode);
            }

            using var doc = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);

            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "[]";

            // Strip residual markdown code fences if the model wraps the JSON
            content = content.Trim();
            if (content.StartsWith("` + '``' + `json")) content = content[7..];
            if (content.StartsWith("` + '``' + `"))     content = content[3..];
            if (content.EndsWith("` + '``' + `"))       content = content[..^3];

            return content.Trim();
        });
    }

    public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new float[768]);
    }

    private static string LoadPrompt(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"StudyPlatform.Infrastructure.Prompts.{fileName}";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Embedded prompt not found: {resourceName}");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action, int maxRetries = 3)
    {
        int retries = 0;
        while (true)
        {
            try { return await action(); }
            catch (Exception) when (retries < maxRetries)
            {
                retries++;
                await Task.Delay((int)Math.Pow(2, retries) * 1000);
            }
        }
    }
}
