namespace BoltonCup.WebAPI.Mapping;

public sealed record GoalBriefDto
{
    public required TimeSpan TimeRemaining { get; set; }
    public required int Period { get; set; }
    public required int TeamId { get; set; }
    public required PlayerBriefDto Scorer  { get; set; }
    public required PlayerBriefDto? PrimaryAssist { get; set; }
    public required PlayerBriefDto? SecondaryAssist { get; set; }

    public string? PeriodLabel => Utilities.Mapping.TryGetPeriodName(Period);
    public string? PeriodAbbreviation => Utilities.Mapping.TryGetPeriodAbbreviation(Period);
    
    public string TimeString => TimeRemaining.ToString(@"mm\:ss");
}