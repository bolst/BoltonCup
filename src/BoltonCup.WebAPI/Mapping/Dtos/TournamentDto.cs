namespace BoltonCup.WebAPI.Mapping;

public record TournamentDto
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public string? Logo { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? WinningTeamId { get; set; }
    public required bool IsActive { get; set; }
    public required bool IsRegistrationOpen { get; set; }
    public required bool IsPaymentOpen { get; set; }
    public int? SkaterLimit { get; set; }
    public int? GoalieLimit { get; set; }
    public GalleryBriefDto? Gallery { get; set; }
}

