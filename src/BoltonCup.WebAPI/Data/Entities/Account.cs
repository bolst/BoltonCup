namespace BoltonCup.WebAPI.Data.Entities;

public class Account : EntityBase
{
    public required Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public required DateTime Birthday { get; set; }
    public string? ProfilePicture { get; set; }
    
    public ICollection<Player> Players { get; set; }
}