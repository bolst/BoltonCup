namespace BoltonCup.Core;

public class Account : EntityBase
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public required DateTime Birthday { get; set; }
    public string? HighestLevel { get; set; }
    public string? Avatar { get; set; }
    public string? Banner { get; set; }
    public string? PreferredBeer { get; set; }
    
    public ICollection<Player> Players { get; set; } = [];
    public ICollection<Team> ManagedTeams { get; set; } = [];
    public ICollection<TournamentRegistration> TournamentRegistrations { get; set; } = [];
    
    public override string ToString() => FirstName + " " + LastName;
}

public class AccountComparer : IEqualityComparer<Account>
{
    public bool Equals(Account? item1, Account? item2)
    {
        if (ReferenceEquals(item1, item2)) 
            return true;
        return item1 is not null && item2 is not null && item1.Id == item2.Id;
    }
        
    public int GetHashCode(Account item) => item.Id;
}