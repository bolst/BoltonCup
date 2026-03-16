namespace BoltonCup.WebAPI.Mapping.Core;

public record PlayerBriefDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string? Position { get; set; }
    public int? JerseyNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime Birthday { get; set; }
    public string? ProfilePicture { get; set; }
    public bool IsGoalie => Position == BoltonCup.Core.Values.Position.Goalie;
    public string FullName => FirstName + " " + LastName;
}