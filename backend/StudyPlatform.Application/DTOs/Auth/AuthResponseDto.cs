namespace StudyPlatform.Application.DTOs.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public object User { get; set; } = null!;
}
