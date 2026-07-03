namespace BoltonCup.Common.Imaging;

public sealed record RosterImageModel
{
    public required string TeamName { get; init; }
    public required string PrimaryHex { get; init; }
    public required string SecondaryHex { get; init; }
    public string? TertiaryHex { get; init; }

    /// <summary>Decoded team logo bytes (any format SkiaSharp can read). Null renders a blank slot.</summary>
    public byte[]? LogoPng { get; init; }

    /// <summary>The font used for all text. Required — the renderer has no system-font fallback.</summary>
    public required byte[] FontTtf { get; init; }

    /// <summary>Per-element colour choices. Defaults to a white-background scheme.</summary>
    public RosterColorway Colorway { get; init; } = new();

    public required IReadOnlyList<RosterPlayerCell> Forwards { get; init; }
    public required IReadOnlyList<RosterPlayerCell> Defense { get; init; }
    public required IReadOnlyList<RosterPlayerCell> Goalies { get; init; }
}

public sealed record RosterPlayerCell
{
    public int? JerseyNumber { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public int? BirthYear { get; init; }

    /// <summary>'C', 'A', or null.</summary>
    public char? Captaincy { get; init; }

    public string Hometown { get; init; } = "WINDSOR, ON";

    /// <summary>
    /// The player's previous-team logo bytes (any format SkiaSharp can read), shown in the block's
    /// mid column. Null renders no logo — the generator supplies the current team logo as a fallback.
    /// </summary>
    public byte[]? PreviousTeamLogoPng { get; init; }

    /// <summary>An empty padding slot (team under-rostered). Renders the frame but no player text.</summary>
    public bool IsEmpty { get; init; }
}
