namespace StudyPlatform.Domain.Entities;

public class Plan
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MonthlyTokens { get; set; }
    public string PriceDisplay { get; set; } = string.Empty;
}
