namespace BoltonCup.Core;

public class GameHighlight : EntityBase
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public int? PlayerId { get; set; }
    public string VideoId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }

    public Game Game { get; set; }
    public Player? Player { get; set; }
}

public class GameHighlightComparer : IEqualityComparer<GameHighlight>
{
    public bool Equals(GameHighlight? item1, GameHighlight? item2)
    {
        if (ReferenceEquals(item1, item2)) 
            return true;
        return item1 is not null && item2 is not null && item1.Id == item2.Id;
    }
        
    public int GetHashCode(GameHighlight item) => item.Id;
}