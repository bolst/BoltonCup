namespace BoltonCup.Core.BracketChallenge;

public class Registration : EntityBase
{
    public Guid Id { get; set; }
    public required Guid BracketChallengeId { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? PaymentId { get; set; }

    public Event BracketChallenge { get; set; } = null!;
}


public class RegistrationComparer : IEqualityComparer<Registration>
{
    public bool Equals(Registration? item1, Registration? item2)
    {
        if (ReferenceEquals(item1, item2)) 
            return true;
        return item1 is not null && item2 is not null && item1.Id == item2.Id;
    }
        
    public int GetHashCode(Registration item) => item.Id.GetHashCode();
}