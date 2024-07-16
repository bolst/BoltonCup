using System.Text.Json;
using System.Text.Json.Serialization;

namespace BoltonCup.Data
{
    public class ScoresheetGame
    {
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("home team")] public string? HomeTeam { get; set; }
        [JsonPropertyName("away team")] public string? AwayTeam { get; set; }
        [JsonPropertyName("date")] public DateTime? Date { get; set; }
        [JsonPropertyName("home penalties")] public List<ScoresheetPenalty>? HomePenalties { get; set; }
        [JsonPropertyName("home goals")] public List<ScoresheetGoal>? HomeGoals { get; set; }
        [JsonPropertyName("away penalties")] public List<ScoresheetPenalty>? AwayPenalties { get; set; }
        [JsonPropertyName("away goals")] public List<ScoresheetGoal>? AwayGoals { get; set; }

        [JsonConstructor] public ScoresheetGame() { }

        public ScoresheetGame(string title, string homeTeam, string awayTeam, DateTime date)
        {
            Title = title;
            HomeTeam = homeTeam;
            AwayTeam = awayTeam;
            Date = date;
            HomePenalties = new();
            HomeGoals = new();
            AwayPenalties = new();
            AwayGoals = new();
        }

        public ScoresheetGame(string strData)
        {
            JsonDocument data = JsonDocument.Parse(strData);
        }

    }
}