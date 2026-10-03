namespace BoltonCup.Core.Commands;

public sealed record CreateFranchiseCommand(
    string Name,
    string NameShort,
    string Abbreviation,
    string PrimaryColorHex,
    string SecondaryColorHex,
    string? TertiaryColorHex
);