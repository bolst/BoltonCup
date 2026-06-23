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

    /// <summary>The configuration path for the account id the API-key principal acts as.</summary>
    public const string AdminAccountIdPath = "BoltonCup:Authentication:AdminAccountId";

    /// <summary>The configuration path for the CIDR ranges the API key may be used from.</summary>
    public const string AllowedNetworksPath = "BoltonCup:Authentication:AdminApiKeyAllowedNetworks";

    /// <summary>HttpContext.Items key holding the true socket peer IP, captured before forwarded-header rewriting.</summary>
    public const string TrueRemoteIpItemKey = "TrueRemoteIp";
}