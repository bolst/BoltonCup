namespace BoltonCup.Core;

public class FranchiseOwner
{
    public int FranchiseId { get; set; }
    public Franchise Franchise { get; set; } = null!;
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
}