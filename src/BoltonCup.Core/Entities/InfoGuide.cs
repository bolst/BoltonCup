namespace BoltonCup.Core;

public class InfoGuide : EntityBase
{
    public required Guid Id { get; set; }
    public string? Title { get; set; }
    public string? MarkdownContent { get; set; }
    public int? TournamentId { get; set; }
    
    public Tournament? Tournament { get; set; }
}

public class InfoGuideComparer : IEqualityComparer<InfoGuide>
{
    public bool Equals(InfoGuide? item1, InfoGuide? item2)
    {
        if (ReferenceEquals(item1, item2)) 
            return true;
        return item1 is not null && item2 is not null && item1.Id == item2.Id;
    }
        
    public int GetHashCode(InfoGuide item) => item.Id.GetHashCode();
}