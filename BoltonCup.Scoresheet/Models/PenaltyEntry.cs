using BoltonCup.Shared.Data;

namespace BoltonCup.Scoresheet.Data;


public class PenaltyEntry
{
    public required BCTeam Team { get; set; }
    public required PlayerProfile Player { get; set; }
    public required string Infraction { get; set; }
}