using System.Text.Json.Serialization;

namespace BoltonCup.Models
{
    public class ScoresheetGoal
    {
        [JsonPropertyName("player number")] public string? PlayerNumber { get; set; }
        [JsonPropertyName("assist1")] public string? Assist1 { get; set; }
        [JsonPropertyName("assist2")] public string? Assist2 { get; set; }
        [JsonPropertyName("time")] public string? Time { get; set; }
        [JsonPropertyName("period")] public string? Period { get; set; }
    }
}