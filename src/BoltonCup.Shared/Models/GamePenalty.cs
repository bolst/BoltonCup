namespace BoltonCup.Shared.Data;

public class GamePenalty
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public TimeSpan Time { get; set; }
    public int Period { get; set; }
    public int TeamId { get; set; }
    public string Infraction { get; set; }
    public int PlayerId { get; set; }
    public string PlayerName { get; set; }
    public string TeamLogo { get; set; }
}