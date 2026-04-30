namespace BoltonCup.Core;

public class TournamentSponsor : EntityBase
{
    public Guid Id { get; init; }
    public int TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;
    
    public string? Name { get; set; }
    public string? Logo { get; set; }
    public string? WebsiteUrl { get; set; }
    public int SortKey { get; set; }
    public bool IsActive { get; set; }
}

public class TournamentSponsorComparer : IEqualityComparer<TournamentSponsor>
{
    public bool Equals(TournamentSponsor? item1, TournamentSponsor? item2)
    {
        if (ReferenceEquals(item1, item2)) 
            return true;
        return item1 is not null && item2 is not null && item1.Id == item2.Id;
    }
        
    public int GetHashCode(TournamentSponsor item) => item.GetHashCode();
}