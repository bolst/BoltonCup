namespace BoltonCup.Core.BracketChallenge;

public class Event : EntityBase
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Link { get; set; }
    public string? Password { get; set; }
    public decimal? Fee { get; set; }
    public bool IsOpen { get; set; }
    public string? Logo { get; set; }

    public ICollection<Registration> Registrations { get; set; } = [];
}

public class EventComparer : IEqualityComparer<Event>
{
    public bool Equals(Event? item1, Event? item2)
    {
        if (ReferenceEquals(item1, item2)) 
            return true;
        return item1 is not null && item2 is not null && item1.Id == item2.Id;
    }
        
    public int GetHashCode(Event item) => item.Id.GetHashCode();
}