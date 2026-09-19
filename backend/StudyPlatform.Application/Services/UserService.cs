using System;
using System.Threading.Tasks;
using StudyPlatform.Domain.Entities;
using StudyPlatform.Domain.Interfaces.Repositories;

namespace StudyPlatform.Application.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPlanRepository _planRepository;
    private readonly JwtService _jwtService;

    public UserService(IUserRepository userRepository, IPlanRepository planRepository, JwtService jwtService)
    {
        _userRepository = userRepository;
        _planRepository = planRepository;
        _jwtService = jwtService;
    }

    public async Task<(string Token, User User)> RegisterAsync(string name, string email, string password)
    {
        var existing = await _userRepository.GetByEmailAsync(email);
        if (existing != null)
            throw new Exception("Email already exists.");

        var plan = await _planRepository.GetByIdAsync(1) 
            ?? new Plan { Id = 1, Name = "Free", MonthlyTokens = 50 };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            PlanId = plan.Id,
            TokenBalance = plan.MonthlyTokens
        };

        await _userRepository.AddAsync(user);

        return (_jwtService.GenerateToken(user), user);
    }

    public async Task<(string Token, User User)> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new Exception("Invalid credentials.");

        return (_jwtService.GenerateToken(user), user);
    }
}
