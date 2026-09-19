using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StudyPlatform.Application.DTOs.Payment;
using StudyPlatform.Domain.Interfaces;
using StudyPlatform.Domain.Interfaces.Repositories;

namespace StudyPlatform.Application.Services;

public class PlanService
{
    private readonly IPaymentProvider _paymentProvider;
    private readonly IPlanRepository _planRepository;
    private readonly IUserRepository _userRepository;

    public PlanService(IPaymentProvider paymentProvider, IPlanRepository planRepository, IUserRepository userRepository)
    {
        _paymentProvider = paymentProvider;
        _planRepository = planRepository;
        _userRepository = userRepository;
    }

    public async Task<bool> SubscribeAsync(Guid userId, int planId)
    {
        return await _paymentProvider.SimulateCheckoutAsync(userId, planId);
    }

    public async Task<List<PlanDto>> GetPlansAsync()
    {
        var plans = await _planRepository.GetAllAsync();
        return plans.Select(p => new PlanDto
        {
            Id = p.Id,
            Name = p.Name,
            MonthlyTokens = p.MonthlyTokens,
            PriceDisplay = p.PriceDisplay
        }).ToList();
    }

    public async Task<BalanceDto?> GetBalanceAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;
        
        var plan = await _planRepository.GetByIdAsync(user.PlanId);

        return new BalanceDto
        {
            TokenBalance = user.TokenBalance,
            PlanId = user.PlanId,
            PlanName = plan?.Name ?? "Unknown"
        };
    }
}