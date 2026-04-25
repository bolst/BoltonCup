namespace BoltonCup.Core;

public class TournamentBudgetItem : EntityBase
{
    public Guid Id { get; init; }
    public int TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;
    public string? Title { get; set; }
    public decimal Amount { get; set; }
    public BudgetItemType BudgetItemType { get; set; }
}

public class TournamentBudgetItemComparer : IEqualityComparer<TournamentBudgetItem>
{
    public bool Equals(TournamentBudgetItem? item1, TournamentBudgetItem? item2)
    {
        if (ReferenceEquals(item1, item2)) 
            return true;
        return item1 is not null && item2 is not null && item1.Id == item2.Id;
    }
        
    public int GetHashCode(TournamentBudgetItem item) => item.GetHashCode();
}