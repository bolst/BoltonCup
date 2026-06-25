namespace BoltonCup.Core;

public class Referee
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public ICollection<Game> Games { get; set; } = [];

    public override string ToString() => FullName;
}

public class RefereeComparer : IEqualityComparer<Referee>
{
    public bool Equals(Referee? item1, Referee? item2)
    {
        if (ReferenceEquals(item1, item2))
            return true;
        return item1 is not null && item2 is not null && item1.Id == item2.Id;
    }

    public int GetHashCode(Referee item) => item.Id;
}
