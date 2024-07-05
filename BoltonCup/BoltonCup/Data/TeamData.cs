using System.Text.Json.Serialization;

namespace BoltonCup.Data
{
    public class TeamData
    {
        [JsonPropertyName("id")] public int? Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("logo")] public string? Logo { get; set; }
        [JsonPropertyName("players")] public List<PlayerData>? Players { get; set; }
    }
}