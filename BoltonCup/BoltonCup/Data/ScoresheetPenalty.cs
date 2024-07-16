using System.Text.Json.Serialization;

namespace BoltonCup.Data
{
    public class ScoresheetPenalty
    {
        [JsonPropertyName("player number")] public string? PlayerNumber { get; set; }
        [JsonPropertyName("infraction")] public string? Infraction { get; set; }
        [JsonPropertyName("minutes")] public string? Minutes { get; set; }
        [JsonPropertyName("time")] public string? Time { get; set; }
        [JsonPropertyName("period")] public string? Period { get; set; }

        public static readonly List<string> INFRACTIONS = new List<string>
        {
            "Boarding",
            "Checking",
            "Charging",
            "Head-Contact",
            "Cross-Checking",
            "Delay-of-Game",
            "Elbowing",
            "Slashing",
            "Roughing",
            "Obstruction",
            "High-Sticking",
            "Kneeing",
            "Holding",
            "Interference",
            "Hooking",
            "Unsportsmanlike",
            "Tripping",
            "Checking-from-Behind",
            "Fighting",
            "Instigating",
            "Spearing"
        };
    }
}