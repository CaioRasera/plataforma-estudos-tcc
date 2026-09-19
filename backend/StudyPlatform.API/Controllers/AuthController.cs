using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StudyPlatform.Application.DTOs.Auth;
using StudyPlatform.Application.Services;

namespace StudyPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;

    public AuthController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto request)
    {
        try
        {
            var (token, user) = await _userService.RegisterAsync(request.Name, request.Email, request.Password);
            return StatusCode(201, new AuthResponseDto 
            { 
                Token = token, 
                User = new { user.Id, user.Name, user.Email, user.TokenBalance, Plan = user.Plan?.Name } 
            });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        try
        {
            var (token, user) = await _userService.LoginAsync(request.Email, request.Password);
            return Ok(new AuthResponseDto 
            { 
                Token = token, 
                User = new { user.Id, user.Name, user.Email, user.TokenBalance, Plan = user.Plan?.Name } 
            });
        }
        catch (System.Exception ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }
}
