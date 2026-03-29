using System.Text.Json.Serialization;

namespace BoltonCup.WebAPI.Errors;

// Stolen from: https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Core/src/ValidationProblemDetails.cs

public class BoltonCupValidationProblemDetails : BoltonCupProblemDetails
{
    public BoltonCupValidationProblemDetails()
        : this(new Dictionary<string, string[]>(StringComparer.Ordinal))
    {
    }

    public BoltonCupValidationProblemDetails(IDictionary<string, string[]> errors)
        : this((IEnumerable<KeyValuePair<string, string[]>>)errors)
    {
    }

    public BoltonCupValidationProblemDetails(IEnumerable<KeyValuePair<string, string[]>> errors)
        : this(new Dictionary<string, string[]>(errors ?? throw new ArgumentNullException(nameof(errors)), StringComparer.Ordinal))
    {
    }

    private BoltonCupValidationProblemDetails(Dictionary<string, string[]> errors)
    {
        Title = "One or more validation errors occurred.";
        Errors = errors;
    }

    [JsonPropertyName("errors")]
    public IDictionary<string, string[]> Errors { get; set; }
}