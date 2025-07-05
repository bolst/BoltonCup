namespace BoltonCup.Shared.Data;

public class BCDraftPickDetail : BCDraftPick
{
    public required string Name { get; set; }
    public required string Birthday { get; set; }
    public required int TeamId { get; set; }
    public required string Position { get; set; }
    public string? ProfilePicture { get; set; }
    public required string TeamName { get; set; }
    public required string TeamNameShort { get; set; }
    public required string TeamLogo { get; set; }
    public required string PrimaryColorHex { get; set; }
    public required string SecondaryColorHex { get; set; }
    public string? TertiaryColorHex { get; set; }
    
}