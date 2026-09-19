using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyPlatform.Application.DTOs.Review;
using StudyPlatform.Application.Services;

namespace StudyPlatform.API.Controllers;

[ApiController]
[Route("api/reviews")]
[Authorize]
public class ReviewsController : ControllerBase
{
    private readonly SpacedRepetitionService _spacedRepetitionService;

    public ReviewsController(SpacedRepetitionService spacedRepetitionService)
    {
        _spacedRepetitionService = spacedRepetitionService;
    }

    [HttpGet("due")]
    public async Task<IActionResult> GetDueReviews([FromQuery] Guid? documentId)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        var due = await _spacedRepetitionService.GetDueReviewsAsync(userId, documentId);
        return Ok(due);
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitReview([FromBody] SubmitReviewDto dto)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        try
        {
            await _spacedRepetitionService.SubmitReviewAsync(userId, dto.ProgressId, dto.Quality);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}