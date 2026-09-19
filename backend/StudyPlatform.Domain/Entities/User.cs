using System;

namespace StudyPlatform.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int PlanId { get; set; }
    public Plan Plan { get; set; } = null!;
    public int TokenBalance { get; set; } = 50;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
