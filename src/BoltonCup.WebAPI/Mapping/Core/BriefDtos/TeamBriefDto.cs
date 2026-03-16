namespace BoltonCup.WebAPI.Mapping.Core;

public record TeamBriefDto
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public string? NameShort { get; set; }
    public string? Abbreviation { get; set; }
    public string? Logo { get; set; }
    public string? Banner { get; set; }
}

public record TeamInGameDto : TeamBriefDto
{
    public int Goals { get; set; }
}
