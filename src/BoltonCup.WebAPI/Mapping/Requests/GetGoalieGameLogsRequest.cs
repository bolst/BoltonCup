namespace BoltonCup.WebAPI.Mapping;

public record GetGoalieGameLogsRequest : RequestBase
{
    public required int GameId { get; set; }
    public string? HomeOrAway { get; set; }
    public int? TeamId { get; set; }
}