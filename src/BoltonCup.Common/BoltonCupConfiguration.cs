namespace BoltonCup.Common;

public class BoltonCupConfiguration
{
    public const string SectionName = "BoltonCup";
    
    public string ApiBaseUrl { get; init; } = string.Empty;
    public string AuthBaseUrl { get; init; } = string.Empty;
    public string WebBaseUrl { get; init; } = string.Empty;
}