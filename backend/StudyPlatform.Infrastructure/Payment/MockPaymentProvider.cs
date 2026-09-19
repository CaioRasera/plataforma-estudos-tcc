using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudyPlatform.Domain.Entities;
using StudyPlatform.Domain.Interfaces;
using StudyPlatform.Infrastructure.Persistence;

namespace StudyPlatform.Infrastructure.Payment;

public class MockPaymentProvider : IPaymentProvider
{
    private readonly AppDbContext _context;

    public MockPaymentProvider(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> SimulateCheckoutAsync(Guid userId, int planId, CancellationToken cancellationToken = default)
    {
        var plan = await _context.Plans.FindAsync(new object[] { planId }, cancellationToken);
        if (plan == null) return false;

        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null) return false;

        user.PlanId = plan.Id;
        user.TokenBalance = plan.MonthlyTokens;

        _context.TokenTransactions.Add(new TokenTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = plan.MonthlyTokens,
            Reason = "plan_purchase",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
