using System.Runtime.CompilerServices;

namespace BoltonCup.Core;

/// <summary>
/// A persistent club identity above the per-tournament <see cref="Team"/> rows. Carries the current brand and
/// optional owners; each season's team keeps its own as-played brand.
/// </summary>
public class Franchise : EntityBase
{
    /// <summary>The slug used when a name leaves nothing usable.</summary>
    public const string SlugFallback = "franchise";

    public int Id { get; set; }
    public required string Name { get; set; }
    /// <summary>URL segment, unique and lowercase. Generated from the name when blank.</summary>
    public string Slug { get; set; } = string.Empty;
    public required string NameShort { get; set; }
    public required string Abbreviation { get; set; }
    public string? Logo { get; set; }
    public string? Banner { get; set; }
    public required string PrimaryColorHex { get; set; }
    public required string SecondaryColorHex { get; set; }
    public string? TertiaryColorHex { get; set; }

    public ICollection<Team> Teams { get; set; } = [];
    public ICollection<Account> Owners { get; set; } = [];

    /// <summary>
    /// Builds a new season team prefilled with this franchise's brand. Only <see cref="Team.FranchiseId"/> is set
    /// (the navigation stays null), and the two objects are independent afterwards.
    /// </summary>
    public Team NewSeasonTeam(int? tournamentId) => new()
    {
        FranchiseId = Id,
        TournamentId = tournamentId,
        Name = Name,
        NameShort = NameShort,
        Abbreviation = Abbreviation,
        Logo = Logo,
        Banner = Banner,
        PrimaryColorHex = PrimaryColorHex,
        SecondaryColorHex = SecondaryColorHex,
        TertiaryColorHex = TertiaryColorHex,
    };

    public override string ToString() => Name;
}

public class FranchiseComparer : IEqualityComparer<Franchise>
{
    public bool Equals(Franchise? item1, Franchise? item2)
    {
        if (ReferenceEquals(item1, item2))
        {
            return true;
        }

        // Unsaved rows all have Id 0, so only reference identity distinguishes them.
        return item1 is not null && item2 is not null && item1.Id != 0 && item1.Id == item2.Id;
    }

    public int GetHashCode(Franchise item) => item.Id == 0 ? RuntimeHelpers.GetHashCode(item) : item.Id;
}