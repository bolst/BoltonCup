namespace BoltonCup.WebAPI.Dtos.Summaries;

public sealed record GoalSummary
{
    public required TimeSpan TimeRemaining { get; set; }
    public required int Period { get; set; }
    public required string PeriodLabel { get; set; }
    public required int TeamId { get; set; }
    public required PlayerSummary Scorer  { get; set; }
    public required PlayerSummary? PrimaryAssist { get; set; }
    public required PlayerSummary? SecondaryAssist { get; set; }
}