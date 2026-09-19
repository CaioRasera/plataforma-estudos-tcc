using System;

namespace StudyPlatform.Application.DTOs.Payment;

public class CheckoutRequestDto
{
    public int PlanId { get; set; }
}

public class PlanDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MonthlyTokens { get; set; }
    public string PriceDisplay { get; set; } = string.Empty;
}

public class BalanceDto
{
    public int TokenBalance { get; set; }
    public int PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
}