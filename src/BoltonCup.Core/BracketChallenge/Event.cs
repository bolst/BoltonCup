namespace BoltonCup.Core.BracketChallenge;

public class Event : EntityBase
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Link { get; set; }
    public string? Password { get; set; }
    public decimal? Fee { get; set; }
    public bool IsOpen { get; set; }

    public ICollection<Registration> Registrations { get; set; } = [];
}