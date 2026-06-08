namespace BoltonCup.Core;

public sealed class PlayerFavourite : EntityBase
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public int DraftId { get; set; }
    public int PlayerId { get; set; }

    public Account Account { get; set; } = null!;
    public Draft Draft { get; set; } = null!;
    public Player Player { get; set; } = null!;
}
