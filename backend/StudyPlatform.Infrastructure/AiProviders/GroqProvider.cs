using System;
using System.Net.Http;
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

    public GroqProvider(IConfiguration config)
    {
        _apiKey = config["AiProvider:Groq:ApiKey"]
            ?? throw new InvalidOperationException("AiProvider:Groq:ApiKey nÃ£o configurado.");
        _model = "openai/gpt-oss-120b";
    }

    public async Task<string> GenerateAsync(string prompt, string? context = null, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            string systemPrompt =
                "VocÃª Ã© um criador de flashcards estritamente acadÃªmico e focado. " +
                "REGRAS DE OURO (OBRIGATÃ“RIAS):\n" +
                "1. IDIOMA: TODO o conteÃºdo gerado (topic, question, answer) DEVE SER EXCLUSIVAMENTE E 100% EM PORTUGUÃŠS DO BRASIL (pt-BR). Traduza absolutamente qualquer termo se necessÃ¡rio.\n" +
                "2. FOCO NO CONTEÃšDO: Gere flashcards APENAS sobre conceitos reais, fatos histÃ³ricos, teorias ou argumentos presentes no texto.\n" +
                "3. PROIBIÃ‡Ã•ES ABSOLUTAS: NUNCA crie perguntas idiotas ou irrelevantes sobre: qual a editora, ano de impressÃ£o, grÃ¡fica, dedicatÃ³ria, sumÃ¡rio, nota de traduÃ§Ã£o, ISBN, pÃ¡ginas ou capas de livros. Ignore isso completamente!\n" +
                "4. IGNORAR LIXO: Se o texto fornecido for APENAS lixo (sumÃ¡rio, informaÃ§Ãµes de grÃ¡fica/editora, Ã­ndice, referÃªncias bibliogrÃ¡ficas vazias), nÃ£o crie NENHUM flashcard. Retorne apenas um array vazio: []\n" +
                "5. TOPIC: O campo \"topic\" deve ser uma ou duas palavras que classifiquem o assunto (ex: \"RevoluÃ§Ã£o\", \"HistÃ³ria\", \"Sociologia\"). Sempre em portuguÃªs.\n" +
                $"INSTRUÃ‡ÃƒO: {prompt}\n" +
                "Formato exato esperado: [{\"topic\":\"...\", \"question\":\"...\", \"answer\":\"...\"}]. Responda APENAS com o array JSON, sem markdown ou explicaÃ§Ãµes.";

            var userContent = context is { Length: > 0 }
                ? $"TEXTO PARA ANÃ LISE:\n{context}"
                : prompt;

            var body = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system",  content = systemPrompt },
                    new { role = "user",    content = userContent  }
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
                throw new HttpRequestException($"Erro Groq ({response.StatusCode}): {errorContent}", null, response.StatusCode);
            }

            using var doc = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);

            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "[]";

            // Limpa possiveis formataÃ§Ãµes markdown residuais
            content = content.Trim();
            if (content.StartsWith("```json")) content = content.Substring(7);
            if (content.StartsWith("```")) content = content.Substring(3);
            if (content.EndsWith("```")) content = content.Substring(0, content.Length - 3);

            return content.Trim();
        });
    }

    public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new float[768]);
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