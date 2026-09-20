using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StudyPlatform.Application.DTOs.Review;
using StudyPlatform.Domain.Entities;
using StudyPlatform.Domain.Interfaces.Repositories;

namespace StudyPlatform.Application.Services;

/// <summary>
/// Implementa o algoritmo SM-2 (Wozniak) de repetição espaçada.
/// </summary>
public class SpacedRepetitionService
{
    private readonly IProgressRepository _progressRepository;
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IDocumentRepository _documentRepository;

    // Previne concorrência ao buscar itens atrasados e inserir novos itens no banco de dados para um mesmo usuário
    // (ex: React StrictMode disparando o useEffect duas vezes e causando deadlock no banco)
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _userLocks = new();

    public SpacedRepetitionService(
        IProgressRepository progressRepository, 
        IAssessmentRepository assessmentRepository,
        IDocumentRepository documentRepository)
    {
        _progressRepository = progressRepository;
        _assessmentRepository = assessmentRepository;
        _documentRepository = documentRepository;
    }

    public async Task<List<ReviewItemDto>> GetDueReviewsAsync(Guid userId, Guid? documentId = null)
    {
        var userLock = _userLocks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));
        await userLock.WaitAsync();

        try
        {
            // 1. Puxar documentos para mapear Título
            var docs = await _documentRepository.GetByUserIdAsync(userId);
            var docMap = docs.ToDictionary(d => d.Id, d => d.Title);

            // 2. Puxar os itens atrasados
            var dueItems = await _progressRepository.GetDueForReviewAsync(userId);
            if (documentId.HasValue)
            {
                dueItems = dueItems.Where(p => p.Item.DocumentId == documentId.Value).ToList();
            }

            // 3. Puxar os itens novos e aplicar limite de 20
            var allItems = await _assessmentRepository.GetByUserIdAsync(userId, documentId);
            var allProgress = await _progressRepository.GetAllByUserIdAsync(userId);
            
            var newItems = allItems
                .Where(a => !allProgress.Any(p => p.ItemId == a.Id))
                .Take(20) // Limite diário de novos itens (prática padrão SRS)
                .ToList();

            var result = dueItems.Select(p => new ReviewItemDto
            {
                ProgressId = p.Id,
                ItemId = p.ItemId,
                DocumentId = p.Item.DocumentId,
                DocumentTitle = docMap.ContainsKey(p.Item.DocumentId) ? docMap[p.Item.DocumentId] : "Documento Desconhecido",
                Type = p.Item.Type,
                Topic = p.Item.Topic,
                Question = p.Item.Question,
                Answer = p.Item.Answer,
                WrongAnswers = p.Item.WrongAnswers
            }).ToList();

            var newProgresses = new List<UserItemProgress>();

            foreach (var newItem in newItems)
            {
                var p = new UserItemProgress
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ItemId = newItem.Id,
                    EasinessFactor = 2.5f,
                    Interval = 0,
                    Repetitions = 0,
                    NextReview = DateTime.UtcNow
                };
                newProgresses.Add(p);

                result.Add(new ReviewItemDto
                {
                    ProgressId = p.Id,
                    ItemId = p.ItemId,
                    DocumentId = newItem.DocumentId,
                    DocumentTitle = docMap.ContainsKey(newItem.DocumentId) ? docMap[newItem.DocumentId] : "Documento Desconhecido",
                    Type = newItem.Type,
                    Topic = newItem.Topic,
                    Question = newItem.Question,
                    Answer = newItem.Answer,
                    WrongAnswers = newItem.WrongAnswers
                });
            }

            if (newProgresses.Any())
            {
                try 
                {
                    await _progressRepository.AddRangeAsync(newProgresses);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao salvar novos progressos. Inner: {ex.InnerException?.Message ?? ex.Message}", ex);
                }
            }

            return result;
        }
        finally
        {
            userLock.Release();
        }
    }

    public async Task SubmitReviewAsync(Guid userId, Guid progressId, int quality)
    {
        if (quality < 0 || quality > 5) throw new ArgumentOutOfRangeException(nameof(quality));

        var progress = await _progressRepository.GetByIdAsync(progressId);
        if (progress == null || progress.UserId != userId) throw new Exception("Progress not found");

        if (quality >= 3)
        {
            if (progress.Repetitions == 0) progress.Interval = 1;
            else if (progress.Repetitions == 1) progress.Interval = 6;
            else progress.Interval = (int)Math.Round(progress.Interval * progress.EasinessFactor);

            progress.Repetitions++;
        }
        else
        {
            progress.Repetitions = 0;
            progress.Interval = 1;
        }

        progress.EasinessFactor = progress.EasinessFactor + (0.1f - (5 - quality) * (0.08f + (5 - quality) * 0.02f));
        if (progress.EasinessFactor < 1.3f) progress.EasinessFactor = 1.3f;

        progress.LastQuality = quality;
        progress.LastReviewed = DateTime.UtcNow;
        progress.NextReview = DateTime.UtcNow.AddDays(progress.Interval);

        await _progressRepository.UpdateAsync(progress);
    }
}
