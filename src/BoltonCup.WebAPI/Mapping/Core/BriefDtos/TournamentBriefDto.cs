namespace BoltonCup.WebAPI.Mapping.Core;

public sealed record TournamentBriefDto
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? WinningTeamId { get; set; }
    public bool IsActive { get; set; }
    public string? Logo { get; set; }
}