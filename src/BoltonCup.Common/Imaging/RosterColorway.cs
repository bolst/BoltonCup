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

    public RosterColor TitleOutline { get; set; } = RosterColor.Secondary;

    public RosterColor BarFill { get; set; } = RosterColor.Primary;
    public RosterColor BarOutline { get; set; } = RosterColor.Black;
    public RosterColor BarText { get; set; } = RosterColor.White;

    public RosterColor JerseyNumber { get; set; } = RosterColor.Primary;
    public RosterColor JerseyNumberOutline { get; set; } = RosterColor.Black;

    public RosterColor PlayerDetailText { get; set; } = RosterColor.Black;

    public RosterColor CaptaincyFill { get; set; } = RosterColor.Primary;
    public RosterColor CaptaincyOutline { get; set; } = RosterColor.Black;
}
