namespace BoltonCup.WebAPI.Mapping;

public sealed record PenaltyBriefDto
{
    public required TimeSpan TimeRemaining { get; set; }
    public required int Period { get; set; }
    public required string PeriodLabel { get; set; }
    public required int TeamId { get; set; }
    public required PlayerBriefDto Player  { get; set; }
    public required string Infraction { get; set; }
    public required int DurationMins { get; set; }
    
    public string TimeString => TimeRemaining.ToString(@"mm\:ss");
}