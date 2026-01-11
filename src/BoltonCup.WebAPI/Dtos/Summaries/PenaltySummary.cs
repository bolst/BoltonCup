namespace BoltonCup.WebAPI.Dtos.Summaries;

public sealed record PenaltySummary
{
    public required TimeSpan TimeRemaining { get; set; }
    public required int Period { get; set; }
    public required string PeriodLabel { get; set; }
    public required int TeamId { get; set; }
    public required PlayerSummary Player  { get; set; }
    public required string Infraction { get; set; }
    public required int DurationMins { get; set; }
}