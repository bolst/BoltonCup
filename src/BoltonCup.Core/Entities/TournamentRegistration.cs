namespace BoltonCup.Core;

public class TournamentRegistration : EntityBase
{
    public Guid Id { get; init; }
    public int TournamentId { get; set; }
    public int AccountId { get; set; }
    public int CurrentStep { get; set; }
    public string? Payload { get; set; }
    public bool IsComplete { get; set; }
    
    public Tournament Tournament { get; set; } = null!;
    public Account Account { get; set; } = null!;

    public override string ToString() => $"{Account.FirstName} {Account.LastName} ({Tournament.Name})";
}

public class TournamentRegistrationComparer : IEqualityComparer<TournamentRegistration>
{
    public bool Equals(TournamentRegistration? item1, TournamentRegistration? item2)
    {
        if (ReferenceEquals(item1, item2)) 
            return true;
        return item1 is not null && item2 is not null && item1.Id == item2.Id;
    }
        
    public int GetHashCode(TournamentRegistration item) => item.GetHashCode();
}