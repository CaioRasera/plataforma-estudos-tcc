using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using StudyPlatform.Application.DTOs.Document;
using StudyPlatform.Domain.Entities;
using StudyPlatform.Domain.Interfaces.Repositories;

namespace StudyPlatform.Application.Services;

public class DocumentService
{
    private readonly IDocumentRepository _documentRepository;

    public DocumentService(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<Document> UploadDocumentAsync(Guid userId, string title, string fileName, Stream fileStream)
    {
        var tempPath = Path.GetTempFileName();
        using (var stream = new FileStream(tempPath, FileMode.Create))
        {
            await fileStream.CopyToAsync(stream);
        }

        var document = new Document
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = string.IsNullOrWhiteSpace(title) ? fileName : title,
            FileName = fileName,
            Status = "Pending"
        };

        await _documentRepository.AddAsync(document);

        var finalPath = Path.Combine(Path.GetTempPath(), document.Id.ToString() + ".pdf");
        File.Move(tempPath, finalPath, true);

        return document;
    }

    public async Task<List<DocumentDto>> GetAllByUserIdAsync(Guid userId)
    {
        var documents = await _documentRepository.GetByUserIdAsync(userId);
        var result = new List<DocumentDto>();
        foreach (var d in documents)
        {
            result.Add(new DocumentDto
            {
                Id = d.Id,
                Title = d.Title,
                FileName = d.FileName,
                Status = d.Status,
                CreatedAt = d.CreatedAt
            });
        }
        return result;
    }

    public async Task<DocumentDto?> GetByIdForUserAsync(Guid documentId, Guid userId)
    {
        var document = await _documentRepository.GetByIdAsync(documentId);
        if (document == null || document.UserId != userId) return null;

        return new DocumentDto
        {
            Id = document.Id,
            Title = document.Title,
            FileName = document.FileName,
            Status = document.Status,
            CreatedAt = document.CreatedAt
        };
    }

    public async Task<bool> DeleteDocumentAsync(Guid documentId, Guid userId)
    {
        var document = await _documentRepository.GetByIdAsync(documentId);
        if (document == null || document.UserId != userId) return false;

        await _documentRepository.DeleteAsync(document);
        
        var filePath = Path.Combine(Path.GetTempPath(), documentId.ToString() + ".pdf");
        if (File.Exists(filePath)) File.Delete(filePath);

        return true;
    }
}

