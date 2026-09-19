using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyPlatform.Application.DTOs.Payment;
using StudyPlatform.Application.Services;

namespace StudyPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly PlanService _planService;

    public PaymentController(PlanService planService)
    {
        _planService = planService;
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequestDto request)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        var success = await _planService.SubscribeAsync(userId, request.PlanId);
        if (success) return Ok(new { success = true, message = "Subscription updated successfully" });

        return BadRequest(new { success = false, message = "Checkout failed" });
    }

    [HttpGet("plans")]
    public async Task<IActionResult> GetPlans()
    {
        var plans = await _planService.GetPlansAsync();
        return Ok(plans);
    }

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        var balance = await _planService.GetBalanceAsync(userId);
        if (balance == null) return NotFound();

        return Ok(balance);
    }
}