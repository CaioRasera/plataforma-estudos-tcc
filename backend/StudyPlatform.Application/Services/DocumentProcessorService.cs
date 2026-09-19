using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pgvector;
using StudyPlatform.Domain.Entities;
using StudyPlatform.Domain.Interfaces;
using StudyPlatform.Domain.Interfaces.Repositories;
using UglyToad.PdfPig;

namespace StudyPlatform.Application.Services;

public class DocumentProcessorService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DocumentProcessorService> _logger;

    public DocumentProcessorService(IServiceScopeFactory scopeFactory, ILogger<DocumentProcessorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task ProcessDocumentAsync(Guid documentId)
    {
        using var scope = _scopeFactory.CreateScope();
        var documentRepo = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();
        var chunkRepo = scope.ServiceProvider.GetRequiredService<IChunkRepository>();
        var assessmentRepo = scope.ServiceProvider.GetRequiredService<IAssessmentRepository>();
        var aiProvider = scope.ServiceProvider.GetRequiredService<IAiProvider>();
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var document = await documentRepo.GetByIdAsync(documentId);
        if (document == null) return;

        var user = await userRepo.GetByIdAsync(document.UserId);
        if (user == null) return;

        try
        {
            document.Status = "Processing";
            await documentRepo.UpdateAsync(document);

            var filePath = Path.Combine(Path.GetTempPath(), documentId.ToString() + ".pdf");
            if (!File.Exists(filePath)) throw new FileNotFoundException("PDF file not found");

            var textChunks = ExtractTextInChunks(filePath);
            document.TotalChunks = textChunks.Count;

            // Calcula proporção de flashcards (1 por ~400 palavras), teto de 80 por doc
            int totalWords = textChunks.Sum(c => c.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length);
            int totalTarget = Math.Clamp(totalWords / 400, 1, 80);
            int targetPerChunk = Math.Max(1, (int)Math.Ceiling((double)totalTarget / textChunks.Count));

            var chunks = new List<Chunk>();
            int index = 0;
            foreach (var text in textChunks)
            {
                var embedding = await aiProvider.EmbedAsync(text);
                var chunk = new Chunk
                {
                    Id = Guid.NewGuid(),
                    DocumentId = document.Id,
                    Content = text,
                    ChunkIndex = index++,
                    PageNumber = 1, // simplified
                    Embedding = new Vector(embedding)
                };
                chunks.Add(chunk);
            }
            await chunkRepo.AddRangeAsync(chunks);

            var assessments = new List<AssessmentItem>();
            int batchSize = 10;
            int delayBetweenBatchesMs = 5000;
            bool outOfTokens = false;

            for (int i = 0; i < chunks.Count; i += batchSize)
            {
                if (outOfTokens) break;

                var batch = chunks.Skip(i).Take(batchSize).ToList();
                _logger.LogInformation($"Processing batch {i / batchSize + 1} of {Math.Ceiling(chunks.Count / (double)batchSize)}...");

                foreach (var chunk in batch)
                {
                    if (user.TokenBalance <= 0)
                    {
                        outOfTokens = true;
                        break;
                    }

                    try
                    {
                        var prompt = $"Gere ATÉ {targetPerChunk} flashcards essenciais a partir deste trecho, respeitando rigidamente as regras de qualidade.";
                        var aiResponse = await aiProvider.GenerateAsync(prompt, chunk.Content);

                        int start = aiResponse.IndexOf('[');
                        int end = aiResponse.LastIndexOf(']');
                        
                        if (start != -1 && end != -1 && end > start)
                        {
                            var cleanJson = aiResponse.Substring(start, end - start + 1);
                            var parsedList = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(
                                cleanJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            
                            if (parsedList != null && parsedList.Count > 0)
                            {
                                foreach (var parsed in parsedList)
                                {
                                    if (user.TokenBalance <= 0)
                                    {
                                        outOfTokens = true;
                                        break;
                                    }

                                    if (parsed.TryGetValue("question", out var jq) && parsed.TryGetValue("answer", out var ja))
                                    {
                                        string q = jq.GetString() ?? "";
                                        string a = ja.GetString() ?? "";
                                        string t = parsed.TryGetValue("topic", out var jt) ? jt.GetString() ?? "Geral" : "Geral";

                                        if (!string.IsNullOrWhiteSpace(q) && !string.IsNullOrWhiteSpace(a))
                                        {
                                            assessments.Add(new AssessmentItem
                                            {
                                                Id = Guid.NewGuid(),
                                                DocumentId = document.Id,
                                                UserId = document.UserId,
                                                Type = "Flashcard",
                                                Question = q,
                                                Answer = a,
                                                Topic = t,
                                                SourceChunkIds = new[] { chunk.Id },
                                                CreatedAt = DateTime.UtcNow
                                            });

                                            user.TokenBalance -= 1;
                                            await userRepo.UpdateAsync(user);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"Array vazio retornado pela IA no chunk {chunk.Id}. Nenhum fato relevante encontrado.");
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Formato incorreto retornado pela IA no chunk {chunk.Id}. IA devolveu: {aiResponse}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Erro processando chunk {chunk.Id}");
                    }
                }

                if (i + batchSize < chunks.Count)
                {
                    await Task.Delay(delayBetweenBatchesMs);
                }
            }

            if (assessments.Any())
            {
                await assessmentRepo.AddRangeAsync(assessments);
                if (outOfTokens)
                {
                    document.Status = "TokensInsufficient";
                }
                else
                {
                    document.Status = "Processed";
                }
            }
            else
            {
                document.Status = "Failed";
            }
            
            document.ProcessedAt = DateTime.UtcNow;
            await documentRepo.UpdateAsync(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing document {DocId}", documentId);
            document.Status = "Failed";
            await documentRepo.UpdateAsync(document);
        }
    }

    private List<string> ExtractTextInChunks(string pdfPath)
    {
        var chunks = new List<string>();
        var fullTextBuilder = new StringBuilder();
        
        using (var pdf = PdfDocument.Open(pdfPath))
        {
            foreach (var page in pdf.GetPages())
            {
                if (!string.IsNullOrWhiteSpace(page.Text))
                {
                    fullTextBuilder.AppendLine(page.Text);
                }
            }
        }

        string fullText = fullTextBuilder.ToString();
        // Quebra inteligente por parágrafos
        var paragraphs = fullText.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

        int maxChunkSize = 4000;
        int overlapSize = 150;
        var currentChunk = new StringBuilder();

        foreach (var p in paragraphs)
        {
            if (currentChunk.Length + p.Length > maxChunkSize && currentChunk.Length > 0)
            {
                string chunkStr = currentChunk.ToString();
                chunks.Add(chunkStr);

                string overlap = chunkStr.Length > overlapSize ? chunkStr.Substring(chunkStr.Length - overlapSize) : chunkStr;
                currentChunk.Clear();
                currentChunk.Append(overlap);
                currentChunk.AppendLine();
            }

            if (p.Length > maxChunkSize)
            {
                // Fallback: se o parágrafo for gigante, corta por tamanho fixo
                for (int i = 0; i < p.Length; i += (maxChunkSize - overlapSize))
                {
                    int length = Math.Min(maxChunkSize - overlapSize, p.Length - i);
                    currentChunk.Append(p.Substring(i, length));
                    
                    if (currentChunk.Length >= maxChunkSize)
                    {
                        string chunkStr = currentChunk.ToString();
                        chunks.Add(chunkStr);
                        string overlap = chunkStr.Length > overlapSize ? chunkStr.Substring(chunkStr.Length - overlapSize) : chunkStr;
                        currentChunk.Clear();
                        currentChunk.Append(overlap);
                    }
                }
            }
            else
            {
                currentChunk.AppendLine(p);
            }
        }

        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString());
        }

        return chunks;
    }
}