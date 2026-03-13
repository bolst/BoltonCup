using BoltonCup.Core;

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

    public PlayerBriefDto(Player player, Account account)
    {
        if (player.AccountId != account.Id)
            throw new ArgumentException("Player account doesn't match given account.");
        
        Id = player.Id;
        AccountId = player.AccountId;
        Position = player.Position;
        JerseyNumber = player.JerseyNumber;
        FirstName = account.FirstName;
        LastName = account.LastName;
        Birthday = account.Birthday;
        ProfilePicture = account.ProfilePicture;
    }
}