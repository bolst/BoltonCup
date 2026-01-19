using System.Linq.Expressions;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.WebAPI.Dtos.Summaries;

namespace BoltonCup.WebAPI.Dtos;


public record PlayerDetailDto : IMappable<Player, PlayerDetailDto>
{
    public required int Id { get; init; }
    public int? AccountId { get; init; }
    public string? Position { get; init; }
    public int? JerseyNumber { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public DateTime? Birthday { get; init; }
    public string? ProfilePicture { get; init; }
    public string? PreferredBeer { get; init; }
    public required TournamentSummary Tournament { get; init; }
    public TeamSummary? Team { get; init; }

    public string FullName => FirstName + " " + LastName;
    public string JerseyNumberLabel => JerseyNumber.HasValue ? $"#{JerseyNumber.Value}" : string.Empty;

    static Expression<Func<Player, PlayerDetailDto>> IMappable<Player, PlayerDetailDto>.Projection =>
        player => new PlayerDetailDto
        {
            Id = player.Id, 
            AccountId = player.AccountId, 
            Position = player.Position, 
            JerseyNumber = player.JerseyNumber, 
            FirstName = player.Account!.FirstName, 
            LastName = player.Account.LastName, 
            Birthday = player.Account.Birthday, 
            ProfilePicture = player.Account.ProfilePicture, 
            PreferredBeer = player.Account.PreferredBeer, 
            Tournament = new TournamentSummary(player.Tournament),
            Team = player.Team == null ? null : new TeamSummary(player.Team),
        };
}
