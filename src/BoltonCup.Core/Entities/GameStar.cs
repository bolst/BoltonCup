namespace BoltonCup.Core;

public class GameStar : EntityBase
{
    public int Id { get; set; }
    public int StarRank { get; set; }
    public int GameId { get; set; }
    public int PlayerId { get; set; }

    public Game Game { get; set; } = null!;
    public Player Player { get; set; } = null!;
}

public class GameStarComparer : IEqualityComparer<GameStar>
{
    public bool Equals(GameStar? item1, GameStar? item2)
    {
        if (ReferenceEquals(item1, item2)) 
            return true;
        return item1 is not null && item2 is not null && item1.Id == item2.Id;
    }
        
    public int GetHashCode(GameStar item) => item.Id;
}