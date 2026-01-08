using System.Linq.Expressions;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.WebAPI.Dtos.Summaries;

namespace BoltonCup.WebAPI.Dtos;


public record PlayerDetailDto : IMappable<Player, PlayerDetailDto>
{
    public int Id { get; set; }
    public int TournamentId { get; set; }
    public int? AccountId { get; set; }
    public string? Position { get; set; }
    public int? JerseyNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? Birthday { get; set; }
    public string? ProfilePicture { get; set; }
    public string? PreferredBeer { get; set; }
    public required string TournamentName { get; set; }
    public TeamSummary? Team { get; set; }

    static Expression<Func<Player, PlayerDetailDto>> IMappable<Player, PlayerDetailDto>.Projection =>
        player => new PlayerDetailDto
        {
            Id = player.Id, 
            TournamentId = player.TournamentId, 
            AccountId = player.AccountId, 
            Position = player.Position, 
            JerseyNumber = player.JerseyNumber, 
            FirstName = player.Account!.FirstName, 
            LastName = player.Account.LastName, 
            Birthday = player.Account.Birthday, 
            ProfilePicture = player.Account.ProfilePicture, 
            PreferredBeer = player.Account.PreferredBeer, 
            TournamentName = player.Tournament.Name, 
            Team = player.Team == null ? null : new TeamSummary(player.Team),
        };
}
