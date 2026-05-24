namespace BoltonCup.WebAPI.Auth;

/// <summary>Constants used by the API key authentication scheme.</summary>
public static class ApiKeyConstants
{
    /// <summary>The name of the API key authentication scheme.</summary>
    public const string Scheme = "APIKey";

    /// <summary>The HTTP header name that carries the API key.</summary>
    public const string Header = "BoltonCup-Api-Key";

    /// <summary>The configuration path where the admin API key is stored.</summary>
    public const string AppSettingsPath = "BoltonCup:Authentication:AdminApiKey";
}