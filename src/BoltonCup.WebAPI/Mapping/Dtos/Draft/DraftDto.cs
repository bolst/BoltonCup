using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public record DraftDto
{
    public required int Id { get; set; }
    public required string? Title { get; set; }
    public required DraftType Type { get; set; }
    public required DraftStatus Status { get; set; }
    public required TournamentBriefDto Tournament { get; set; }
}