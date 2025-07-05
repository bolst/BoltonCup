namespace BoltonCup.Shared.Data;

public class PenaltyEntry
{
    public required BCGame Game { get; set; }
    public required BCTeam Team { get; set; }
    public required PlayerProfile Player { get; set; }
    public required string Infraction { get; set; }
    public string Notes { get; set; } = string.Empty;
    public required int DurationMins { get; set; }
    public required int Period { get; set; }
    public required TimeSpan Time { get; set; }
}