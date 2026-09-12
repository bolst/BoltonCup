using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public partial class Mapper
{
    // ---------- Standings ----------

    public TournamentStandingsDto ToDto(TournamentStandings standings) => new TournamentStandingsDto
    {
        RoundRobin = standings.RoundRobin.Select(ToStandingRowDto).ToList(),
        Playoffs = standings.Playoffs.Select(ToStandingRowDto).ToList(),
    };

    StandingRowDto ToStandingRowDto(StandingRow row) => new()
    {
        Rank = row.Rank,
        TeamId = row.Team.Id,
        Name = row.Team.Name,
        NameShort = row.Team.NameShort,
        Abbreviation = row.Team.Abbreviation,
        Logo = _urlResolver.GetFullUrl(row.Team.Logo),
        PrimaryColorHex = row.Team.PrimaryColorHex,
        SecondaryColorHex = row.Team.SecondaryColorHex,
        TertiaryColorHex = row.Team.TertiaryColorHex,
        GamesPlayed = row.GamesPlayed,
        Wins = row.Wins,
        RegulationWins = row.RegulationWins,
        Losses = row.Losses,
        Ties = row.Ties,
        OtSoLosses = row.OtSoLosses,
        GoalsFor = row.GoalsFor,
        GoalsAgainst = row.GoalsAgainst,
        GoalDifferential = row.GoalDifferential,
        Points = row.Points,
    };
}
