using System.Text.Json.Serialization;

namespace BoltonCup.Models
{
    public class TeamData
    {
        [JsonPropertyName("id")] public int? Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("logo")] public string? Logo { get; set; }
        [JsonPropertyName("players")] public List<TeamPlayer>? Players { get; set; }
    }
}