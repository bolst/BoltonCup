using BoltonCup.Core;

namespace BoltonCup.WebAPI.Dtos.Summaries;

public record TournamentSummary
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? WinningTeamId { get; set; }
    public bool IsActive { get; set; }

    public TournamentSummary(Tournament tournament)
    {
        Id = tournament.Id;
        Name = tournament.Name;
        StartDate = tournament.StartDate;
        EndDate = tournament.EndDate;
        WinningTeamId = tournament.WinningTeamId;
        IsActive = tournament.IsActive;
    }
}