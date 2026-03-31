namespace BoltonCup.WebAPI.Mapping;

public record GetSkaterGameLogsRequest : RequestBase
{
    public required int GameId { get; set; }
    public string? HomeOrAway { get; set; }
    public int? TeamId { get; set; }
    public string? Position { get; set; }
}