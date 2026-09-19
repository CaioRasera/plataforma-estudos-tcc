using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudyPlatform.Application.DTOs.Document;
using StudyPlatform.Application.Services;

namespace StudyPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly DocumentService _documentService;
    private readonly DocumentProcessorService _processorService;

    public DocumentsController(DocumentService documentService, DocumentProcessorService processorService)
    {
        _documentService = documentService;
        _processorService = processorService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, [FromForm] string? title)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "Nenhum arquivo enviado." });

        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        var document = await _documentService.UploadDocumentAsync(userId, title ?? string.Empty, file.FileName, file.OpenReadStream());
        _ = Task.Run(() => _processorService.ProcessDocumentAsync(document.Id));

        return Accepted(new UploadResponseDto { DocumentId = document.Id, Message = "Upload recebido. Processamento iniciado." });
    }

    [HttpGet]
    public async Task<IActionResult> GetMyDocuments()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        var documents = await _documentService.GetAllByUserIdAsync(userId);
        return Ok(documents);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDocument(Guid id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        var document = await _documentService.GetByIdForUserAsync(id, userId);
        if (document == null) return NotFound();

        return Ok(document);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocument(Guid id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        var success = await _documentService.DeleteDocumentAsync(id, userId);
        if (!success) return NotFound();

        return NoContent();
    }
}