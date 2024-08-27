using System.Text.Json.Serialization;

namespace BoltonCup.Models
{
    public class TeamPlayer
    {
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("birth year")] public string? BirthYear { get; set; }
        [JsonPropertyName("position")] public string? Position { get; set; }
        [JsonPropertyName("number")] public string? Number { get; set; }

        public string PosAbbrev()
        {
            if (Position is null) return "N/A";
            return Position.Contains("/") ? "F/D" : Position.First().ToString();
        }
    }
}