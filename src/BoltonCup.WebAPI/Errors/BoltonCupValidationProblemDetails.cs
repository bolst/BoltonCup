using System.Text.Json.Serialization;

namespace BoltonCup.WebAPI.Errors;

// Stolen from: https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Core/src/ValidationProblemDetails.cs

/// <summary>Represents structured validation problem details with field-level errors.</summary>
public class BoltonCupValidationProblemDetails : BoltonCupProblemDetails
{
    /// <summary>Initializes a new instance of <see cref="BoltonCupValidationProblemDetails"/>.</summary>
    public BoltonCupValidationProblemDetails()
        : this(new Dictionary<string, string[]>(StringComparer.Ordinal))
    {
    }

    /// <summary>Initializes a new instance of <see cref="BoltonCupValidationProblemDetails"/> with the provided errors dictionary.</summary>
    public BoltonCupValidationProblemDetails(IDictionary<string, string[]> errors)
        : this((IEnumerable<KeyValuePair<string, string[]>>)errors)
    {
    }

    /// <summary>Initializes a new instance of <see cref="BoltonCupValidationProblemDetails"/> from a sequence of field-error pairs.</summary>
    public BoltonCupValidationProblemDetails(IEnumerable<KeyValuePair<string, string[]>> errors)
        : this(new Dictionary<string, string[]>(errors ?? throw new ArgumentNullException(nameof(errors)), StringComparer.Ordinal))
    {
    }

    private BoltonCupValidationProblemDetails(Dictionary<string, string[]> errors)
    {
        Title = "One or more validation errors occurred.";
        Errors = errors;
    }

    /// <summary>The validation errors keyed by field name.</summary>
    [JsonPropertyName("errors")]
    public IDictionary<string, string[]> Errors { get; set; }
}