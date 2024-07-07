using System.Text.Json.Serialization;

namespace BoltonCup.Data
{
    public class ScoresData
    {
        [JsonPropertyName("scores")] public List<string>? Scores { get; set; }
    }
}