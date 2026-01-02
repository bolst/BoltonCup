namespace BoltonCup.WebAPI.Data.Entities;

public class Account : EntityBase
{
    public required int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public required DateTime Birthday { get; set; }
    public string? HighestLevel { get; set; }
    public string? ProfilePicture { get; set; }
    public string? PreferredBeer { get; set; }
    
    public ICollection<Player> Players { get; set; }
    public ICollection<Team> ManagedTeams { get; set; }
}