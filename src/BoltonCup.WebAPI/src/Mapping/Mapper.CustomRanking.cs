using System.Security.Claims;
using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Shared;

namespace BoltonCup.WebAPI.Mapping;

public partial class Mapper
{
    // ---------- CustomRanking ----------

    public IReadOnlyList<CustomRankingDto> ToDtoList(IReadOnlyList<CustomRanking> rankings) => rankings
            .Select(ranking => new CustomRankingDto
            {
                Id = ranking.Id,
                Title = ranking.Title,
                Tournament = ToTournamentBriefDto(ranking.Tournament),
                PlayerCount = ranking.Players.Count,
                CreatedByName = AccountName(ranking.Account),
                CreatedAt = ranking.CreatedAt,
            })
            .ToList();

    public CustomRankingSingleDto? ToDto(CustomRanking? ranking, bool canEdit, IReadOnlySet<int>? stalePlayerIds = null)
    {
        if (ranking is null)
        {
            return null;
        }

        return new CustomRankingSingleDto
        {
            Id = ranking.Id,
            Title = ranking.Title,
            Tournament = ToTournamentBriefDto(ranking.Tournament),
            CreatedByName = AccountName(ranking.Account),
            Players = ranking.Players
                .OrderBy(p => p.Rank)
                .Select(p => new CustomRankingPlayerDto
                {
                    Rank = p.Rank,
                    IsStale = stalePlayerIds?.Contains(p.PlayerId) ?? false,
                    Player = ToPlayerBriefDto(p.Player),
                    GamesPlayed = p.GamesPlayed,
                    TotalPoints = p.TotalPoints,
                    PointsPerGame = p.PointsPerGame,
                })
                .ToList(),
            CanEdit = canEdit,
        };
    }

    public IReadOnlyList<CustomRankingShareDto> ToShareDtoList(IReadOnlyList<CustomRankingShareInfo> shares) => shares
            .Select(s => new CustomRankingShareDto
            {
                AccountId = s.AccountId,
                Name = s.Name,
                Email = s.Email,
                Avatar = s.Avatar,
            })
            .ToList();

    public IReadOnlyList<RankingInviteUserDto> ToInviteDtoList(IReadOnlyList<RankingInviteCandidate> candidates) => candidates
            .Select(c => new RankingInviteUserDto
            {
                AccountId = c.AccountId,
                Name = c.Name,
                Email = c.Email,
            })
            .ToList();

    public CreateCustomRankingCommand ToCommand(CreateCustomRankingRequest request, ClaimsPrincipal user) => new CreateCustomRankingCommand(
            TournamentId: request.TournamentId,
            Title: request.Title,
            OwnerAccountId: user.GetAccountId()
        );

    public UpdateCustomRankingCommand ToCommand(UpdateCustomRankingRequest request) => new UpdateCustomRankingCommand(
            Title: request.Title,
            OrderedPlayerIds: request.OrderedPlayerIds
        );
}
