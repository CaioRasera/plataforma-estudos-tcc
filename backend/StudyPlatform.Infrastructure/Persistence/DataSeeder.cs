using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudyPlatform.Domain.Entities;

namespace StudyPlatform.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!context.Plans.Any())
        {
            context.Plans.AddRange(
                new Plan { Id = 1, Name = "Free", MonthlyTokens = 50, PriceDisplay = "R$ 0" },
                new Plan { Id = 2, Name = "Plus", MonthlyTokens = 300, PriceDisplay = "R$ 19,90" },
                new Plan { Id = 3, Name = "Pro", MonthlyTokens = 1000, PriceDisplay = "R$ 49,90" }
            );
            await context.SaveChangesAsync();
        }

        if (!context.Users.Any(u => u.Email == "demo@estudos.local"))
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Demo User",
                Email = "demo@estudos.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo@123"),
                PlanId = 1,
                TokenBalance = 50
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }
    }
}