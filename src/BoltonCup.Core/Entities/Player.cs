namespace BoltonCup.Core;

public class Player : EntityBase
{
    public required int Id { get; set; }
    public required int TournamentId { get; set; }
    public int? AccountId { get; set; }
    public int? TeamId { get; set; }
    public string? Position { get; set; }
    public int? JerseyNumber { get; set; }
    
    public Account? Account { get; set; }
    public Tournament Tournament { get; set; }
    public Team? Team { get; set; }
    public ICollection<Goal> Goals { get; set; }
    public ICollection <Goal> PrimaryAssists { get; set; }
    public ICollection <Goal> SecondaryAssists { get; set; }
    public ICollection<Penalty> Penalties { get; set; }
}