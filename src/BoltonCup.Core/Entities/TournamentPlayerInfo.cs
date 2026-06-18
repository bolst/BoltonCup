namespace BoltonCup.Core;

public class TournamentPlayerInfo : EntityBase
{
    public Guid Id { get; init; }
    public int TournamentId { get; set; }
    public int AccountId { get; set; }
    public string? Payload { get; set; }

    public Tournament Tournament { get; set; } = null!;
    public Account Account { get; set; } = null!;

    public override string ToString() => $"{Account.FirstName} {Account.LastName} ({Tournament.Name})";
}

public class TournamentPlayerInfoComparer : IEqualityComparer<TournamentPlayerInfo>
{
    public bool Equals(TournamentPlayerInfo? item1, TournamentPlayerInfo? item2)
    {
        if (ReferenceEquals(item1, item2))
            return true;
        return item1 is not null && item2 is not null && item1.Id == item2.Id;
    }

    public int GetHashCode(TournamentPlayerInfo item) => item.Id.GetHashCode();
}
