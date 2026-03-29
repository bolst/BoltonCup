using System.Text.Json.Serialization;

namespace BoltonCup.WebAPI.Errors;

// Stolen from: https://github.com/dotnet/aspnetcore/blob/main/src/Http/Http.Abstractions/src/ProblemDetails/ProblemDetails.cs

public class BoltonCupProblemDetails
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(-5)]
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(-4)]
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(-3)]
    [JsonPropertyName("status")]
    public int? Status { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(-2)]
    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(-1)]
    [JsonPropertyName("instance")]
    public string? Instance { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object?> Extensions { get; set; } = new Dictionary<string, object?>(StringComparer.Ordinal);
}