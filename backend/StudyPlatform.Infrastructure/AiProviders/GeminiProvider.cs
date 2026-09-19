using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StudyPlatform.Domain.Interfaces;

namespace StudyPlatform.Infrastructure.AiProviders;

public class GeminiProvider : IAiProvider
{
    private static readonly HttpClient _http = new();
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _embeddingModel;

    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";

    public GeminiProvider(IConfiguration config)
    {
        _apiKey = config["AiProvider:Gemini:ApiKey"]
            ?? throw new InvalidOperationException("AiProvider:Gemini:ApiKey não configurado.");
        _model = config["AiProvider:Gemini:Model"] ?? "gemini-3.5-flash";
        _embeddingModel = config["AiProvider:Gemini:EmbeddingModel"] ?? "gemini-embedding-2";
    }

    public async Task<string> GenerateAsync(string prompt, string? context = null, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            const string systemInstruction =
                "Você é um criador de flashcards. Extraia as informações mais importantes do texto. " +
                "Você DEVE retornar ESTRITAMENTE um ARRAY JSON contendo até 5 flashcards importantes. " +
                "Formato exato: [{\"question\":\"sua pergunta\", \"answer\":\"sua resposta\"}]. Não use markdown.";

            var userContent = context is { Length: > 0 }
                ? $"TEXTO:\n{context}"
                : prompt;

            var body = new
            {
                system_instruction = new { parts = new[] { new { text = systemInstruction } } },
                contents = new[] { new { parts = new[] { new { text = userContent } } } },
                generationConfig = new { 
                    temperature = 0.3, 
                    maxOutputTokens = 2048,
                    responseMimeType = "application/json"
                }
            };

            var url = $"{BaseUrl}/{_model}:generateContent?key={_apiKey}";
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            using var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Erro Gemini ({response.StatusCode}): {errorContent}", null, response.StatusCode);
            }

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            
            try 
            {
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "{}";
                return text.Trim();
            } 
            catch 
            {
                return "{}";
            }
        });
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var body = new { content = new { parts = new[] { new { text } } } };
            var url = $"{BaseUrl}/{_embeddingModel}:embedContent?key={_apiKey}";

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            using var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Erro Gemini Embedding ({response.StatusCode}): {errorContent}", null, response.StatusCode);
            }

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);

            var values = doc.RootElement.GetProperty("embedding").GetProperty("values");
            var arr = new float[768];
            int i = 0;
            foreach (var v in values.EnumerateArray())
            {
                if (i >= 768) break;
                arr[i++] = v.GetSingle();
            }
            return arr;
        });
    }

    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action, int maxRetries = 5)
    {
        int retries = 0;
        while (true)
        {
            try { return await action(); }
            catch (HttpRequestException ex) when ((int)ex.StatusCode == 429 && retries < maxRetries)
            {
                retries++;
                // Rate limit reached: wait 45s to ensure quota resets before retrying.
                await Task.Delay(45000);
            }
            catch (Exception) when (retries < maxRetries)
            {
                retries++;
                await Task.Delay((int)Math.Pow(2, retries) * 1000);
            }
        }
    }
}