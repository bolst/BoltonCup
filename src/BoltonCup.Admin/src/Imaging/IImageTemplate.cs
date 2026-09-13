namespace BoltonCup.Admin.Imaging;

/// <summary>
/// Metadata describing a selectable image template on the Image Generation page.
/// Add a new implementation (and register it in Program.cs) to expose another template.
/// </summary>
public interface IImageTemplate
{
    string Key { get; }
    string DisplayName { get; }
}

public sealed class TeamRosterTemplate : IImageTemplate
{
    public const string TemplateKey = "team-roster";
    public string Key => TemplateKey;
    public string DisplayName => "Team Roster";
}