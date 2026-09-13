using System.Text.Json.Serialization;

namespace BoltonCup.WebAPI.Errors;

// Stolen from: https://github.com/dotnet/aspnetcore/blob/main/src/Http/Http.Abstractions/src/ProblemDetails/ProblemDetails.cs
/// <summary>Represents structured problem details returned by the API for errors.</summary>
public class BoltonCupProblemDetails
{
    /// <summary>A URI reference identifying the problem type.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(-5)]
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>A short, human-readable summary of the problem type.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(-4)]
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>The HTTP status code for this problem.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(-3)]
    [JsonPropertyName("status")]
    public int? Status { get; set; }

    /// <summary>A human-readable explanation specific to this occurrence of the problem.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(-2)]
    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    /// <summary>A URI reference identifying the specific occurrence of the problem.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(-1)]
    [JsonPropertyName("instance")]
    public string? Instance { get; set; }

    /// <summary>Additional properties that may be included in the problem details.</summary>
    [JsonExtensionData]
    public IDictionary<string, object?> Extensions { get; set; } = new Dictionary<string, object?>(StringComparer.Ordinal);
}