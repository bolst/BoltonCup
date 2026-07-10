namespace BoltonCup.Common.Imaging;

/// <summary>A selectable colour source for a roster card element.</summary>
public enum RosterColor
{
    Primary,
    Secondary,
    Tertiary,
    White,
    Black,
}

/// <summary>
/// Per-element colour choices for the roster card. Each element resolves to one of the
/// team colours (primary/secondary/tertiary) or plain white/black at render time.
/// </summary>
public sealed class RosterColorway
{
    public RosterColor Background { get; set; } = RosterColor.White;

    // Team name (the header title). Fill/outline apply to the team-name glyphs.
    public RosterColor TitleFill { get; set; } = RosterColor.Primary;
    public RosterColor TitleOutline { get; set; } = RosterColor.Secondary;

    // Position title bar (FORWARDS / DEFENSE / GOALIE). Background is the bar rectangle,
    // outline is its border, and fill is the label text drawn on top.
    public RosterColor PositionBarBackground { get; set; } = RosterColor.Primary;
    public RosterColor PositionBarOutline { get; set; } = RosterColor.Black;
    public RosterColor PositionBarFill { get; set; } = RosterColor.White;

    public RosterColor JerseyNumber { get; set; } = RosterColor.Primary;
    public RosterColor JerseyNumberOutline { get; set; } = RosterColor.Black;

    public RosterColor PlayerDetailText { get; set; } = RosterColor.Black;

    public RosterColor CaptaincyFill { get; set; } = RosterColor.Primary;
    public RosterColor CaptaincyOutline { get; set; } = RosterColor.Black;
}
