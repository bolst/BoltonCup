using System.Security.Claims;
using BoltonCup.Core;
using BoltonCup.Core.BracketChallenge;
using BoltonCup.Core.Commands;
using BoltonCup.Infrastructure.Identity;
using BoltonCup.Shared;
using Stripe;
using Account = BoltonCup.Core.Account;
using Event = BoltonCup.Core.BracketChallenge.Event;

#pragma warning disable CS1591

namespace BoltonCup.WebAPI.Mapping;

public class Mapper : IMapper
{
    private readonly IAssetUrlResolver _urlResolver;
    
    public Mapper(IAssetUrlResolver urlResolver)
    {
        _urlResolver = urlResolver;
    }
    
    // ---------- Account ----------

    public AccountDto? ToDto(Account? account, ClaimsPrincipal claims)
    {
        if (account?.Id is null)
            return null;
        return new AccountDto
        {
            Id = account.Id,
            Email = account.Email,
            FirstName = account.FirstName,
            LastName = account.LastName,
            Name = (account.FirstName + " " + account.LastName).Trim(),
            Phone = account.Phone ?? claims.FindFirstValue(ClaimTypes.MobilePhone),
            Birthday = account.Birthday,
            HighestLevel = account.HighestLevel,
            Avatar = account.Avatar,
            Banner = account.Banner,
            PreferredBeer = account.PreferredBeer,
            HeightFeet = account.HeightFeet,
            HeightInches = account.HeightInches,
            Weight = account.Weight
        };
    }

    public ICollection<AccountTournamentDto> ToAccountTournamentDtoList(Account? account)
    {
        if (account is null)
            return [];
        return account.Players
            .Select(player => new AccountTournamentDto
            {
                Tournament = ToTournamentBriefDto(player.Tournament),
                Team = player.Team == null ? null : ToTeamBriefDto(player.Team)
            })
            .OrderByDescending(x => x.Tournament.StartDate)
            .ToList();
    }

    public CreateAccountCommand ToCommand(CompleteUserAccountRequest request, ClaimsPrincipal claims)
    {
        return new CreateAccountCommand(
            FirstName: request.FirstName,
            LastName: request.LastName,
            Email: claims.FindFirstValue(ClaimTypes.Email) ?? throw new InvalidOperationException("Missing email claim"),
            Birthday: request.Birthday,
            HeightFeet: request.HeightFeet,
            HeightInches: request.HeightInches,
            Weight: request.Weight,
            HighestLevel: request.HighestLevel,
            PreferredBeer: request.PreferredBeer
        );
    }

    public UpdateAccountCommand ToCommand(UpdateAccountRequest request, ClaimsPrincipal claims)
    {
        var (feet, inches) = ParseHeight(request.Height);
        var accountId = claims.GetAccountId();
        return new UpdateAccountCommand(
            AccountId: accountId,
            FirstName: request.FirstName,
            LastName: request.LastName,
            Birthday: request.Birthday,
            HighestLevel: request.HighestLevel,
            PreferredBeer: request.PreferredBeer,
            HeightFeet: feet,
            HeightInches: inches,
            Weight: request.Weight
        );
    }

    private static (int? Feet, int? Inches) ParseHeight(string? height)
    {
        if (string.IsNullOrEmpty(height))
            return (null, null);

        var data = height.Split("'");
        if (data is not [var feetStr, var inchesStr, ..])
            return (null, null);

        if (!int.TryParse(feetStr, out var feet) || !int.TryParse(inchesStr, out var inches))
            return (null, null);

        return (feet, inches);
    }


    // ---------- User ----------

    public CurrentUserDto? ToDto(ClaimsPrincipal claims)
    {
        var userId = claims.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrEmpty(userId)
            ? null
            : new CurrentUserDto(
                UserId: userId,
                Email: claims.FindFirstValue(ClaimTypes.Email) ?? "",
                Name: claims.FindFirstValue(ClaimTypes.Name) ?? "",
                Roles: claims.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList(),
                IsAuthenticated: claims.Identity?.IsAuthenticated ?? false,
                AccountId: claims.GetAccountIdOrDefault(),
                TeamGmIds: claims.GetTeamGmIds(),
                TournamentGmIds: claims.GetTournamentGmIds()
            );
    }


    // ---------- BracketChallenge ----------

    public GetBracketChallengesQuery ToQuery(GetBracketChallengesRequest request)
    {
        return new GetBracketChallengesQuery
        {
            Page = request.Page,
            Size = request.Size,
            SortBy = request.SortBy,
            Descending = request.Descending,
        };
    }

    public IPagedList<BracketChallengeDto> ToDtoList(IPagedList<Event> bracketChallenges)
    {
        return bracketChallenges.ProjectTo(challenge => new BracketChallengeDto
        {
            Id = challenge.Id,
            Title = challenge.Title,
            Link = challenge.Link,
            Fee = challenge.Fee,
            IsOpen = challenge.IsOpen,
            Logo = _urlResolver.GetFullUrl(challenge.Logo),
            CloseDate = challenge.RegistrationCloseDate
        });
    }

    public BracketChallengeSingleDto? ToDto(Event? challenge)
    {
        if (challenge is null)
            return null;
        return new BracketChallengeSingleDto
        {
            Id = challenge.Id,
            Title = challenge.Title,
            Link = challenge.Link,
            Fee = challenge.Fee,
            IsOpen = challenge.IsOpen,
            Logo = _urlResolver.GetFullUrl(challenge.Logo),
            CloseDate = challenge.RegistrationCloseDate,
            TOSMarkdown = challenge.TermsOfServiceMarkdownContent
        };
    }

    public BracketChallengePaymentIntentDto ToDto(BracketChallengePaymentIntent paymentIntent)
    {
        return new BracketChallengePaymentIntentDto(
            ClientSecret: paymentIntent.Secret,
            TotalAmount: paymentIntent.Amount,
            Currency: paymentIntent.Currency,
            Breakdown: paymentIntent.AmountBreakdown
        );
    }

    public CreateBracketChallengePaymentIntentCommand ToCommand(int bracketChallengeId, CreateBracketChallengePaymentIntentRequest request)
    {
        return new CreateBracketChallengePaymentIntentCommand(
            Name: request.Name,
            Email: request.Email,
            AgreedToTOS: request.AgreedToTOS,
            BracketChallengeId: bracketChallengeId
        );
    }


    // ---------- Draft ----------

    public IPagedList<DraftDto> ToDtoList(IPagedList<Draft> drafts)
    {
        return drafts.ProjectTo(draft => new DraftDto
        {
            Id = draft.Id,
            Title = draft.Title,
            Type = draft.Type,
            Status = draft.Status,
            Tournament = ToTournamentBriefDto(draft.Tournament),
            IsVisible = draft.IsVisible,
            Rounds = draft.Rounds,
            Teams = draft.Teams,
            SecondsPerPick = draft.SecondsPerPick,
        });
    }

    public IPagedList<DraftPickDto> ToDtoList(IPagedList<DraftPick> draftPicks)
    {
        return draftPicks.ProjectTo(ToDraftPickDto);
    }

    public IPagedList<DraftRankingDto> ToDtoList(IPagedList<PlayerDraftRanking> rankings, IReadOnlySet<int> favouritePlayerIds)
    {
        return rankings.ProjectTo(draft => new DraftRankingDto
        {
            Id = draft.Id,
            DraftId = draft.DraftId,
            TournamentId = draft.TournamentId,
            PlayerPhone = draft.Player.Account.Phone,
            Player = ToPlayerBriefDto(draft.Player),
            DraftPick = ToDraftPickBriefDto(draft.DraftPick),
            GamesPlayed = draft.GamesPlayed,
            TotalPoints = draft.TotalPoints,
            DraftRanking = draft.DraftRanking,
            OverrideRanking = draft.OverrideRanking,
            IsDrafted = draft.IsDrafted,
            PointsPerGame = draft.PointsPerGame,
            IsFavourite = favouritePlayerIds.Contains(draft.PlayerId),
        });
    }

    public DraftSingleDto? ToDto(Draft? draft, bool isAuthorized, bool canManage)
    {
        if (draft is null)
            return null;
        return new DraftSingleDto
        {
            Id = draft.Id,
            Title = draft.Title,
            Type = draft.Type,
            Status = draft.Status,
            IsVisible = draft.IsVisible,
            Rounds = draft.Rounds,
            Teams = draft.Teams,
            SecondsPerPick = draft.SecondsPerPick,
            Tournament = ToTournamentBriefDto(draft.Tournament),
            PickOrder = draft.DraftOrders
                .Select(order => new DraftPickOrderDto
                {
                    Pick = order.Pick,
                    Team = ToTeamBriefDto(order.Team),
                    AutoPick = order.AutoPick
                })
                .OrderBy(d => d.Pick),
            DraftPicksByRound = draft.DraftPicks
                .GroupBy(dto => dto.Round)
                .Select(group => new RoundDraftPicks(group.Key, group.Select(ToDraftPickDto).OrderBy(x => x.RoundPick)))
                .OrderBy(group => group.Round),
            CanEditDraft = isAuthorized && draft.Status != DraftStatus.Completed,
            CanManageDraft = canManage,
            DefaultCustomRankingId = draft.DefaultCustomRankingId,
        };
    }

    public DraftPickSingleDto? ToDto(DraftPick? draftPick)
    {
        if (draftPick is null)
            return null;
        return new DraftPickSingleDto
        {
            DraftId = draftPick.DraftId,
            OverallPick = draftPick.OverallPick,
            Round = draftPick.Round,
            RoundPick = draftPick.RoundPick,
            Team = ToTeamBriefDto(draftPick.Team),
            Player = draftPick.Player is null ? null : ToPlayerBriefDto(draftPick.Player),
            ClockStartedAt = draftPick.ClockStartedAt,
        };
    }

    public DraftUpdateEventDto ToDto(CurrentDraftState draftState, bool isAuthorized, bool canManage)
    {
        return new DraftUpdateEventDto(
            Draft: ToDto(draftState.Draft, isAuthorized, canManage)!,
            NextPick: ToDto(draftState.NextPick)
        );
    }

    public DraftPickMadeEventDto ToDto(CurrentDraftStateWithPick draftState)
    {
        return new DraftPickMadeEventDto(
            DraftId: draftState.Draft.Id,
            CompletedPick: ToDraftPickBriefDto(draftState.CompletedPick)!,
            DraftedPlayer: ToPlayerBriefDto(draftState.CompletedPick!.Player!),
            NextPick: ToDto(draftState.NextPick)
        );
    }

    public GetDraftsQuery ToQuery(GetDraftsRequest request, ClaimsPrincipal user)
    {
        return new GetDraftsQuery
        {
            TournamentId = request.TournamentId,
            Status = request.Status,
            AccountId = user.GetAccountIdOrDefault(),
            IsAdmin = user.IsInRole(BoltonCupRole.Admin),
        };
    }

    public CreateDraftCommand ToCommand(CreateDraftRequest request, ClaimsPrincipal user)
    {
        return new CreateDraftCommand(
            TournamentId: request.TournamentId,
            Title: request.Title,
            OwnerAccountId: user.GetAccountIdOrDefault()
        );
    }

    public UpdateDraftCommand ToCommand(UpdateDraftRequest request)
    {
        return new UpdateDraftCommand
        {
            Title = request.Title,
            DraftType = request.DraftType,
            Ordering = request.Ordering?
                .Select(x => new DraftOrderCommandEntry(x.TeamId, x.Pick))
                .ToList(),
            IsVisible = request.IsVisible,
            SecondsPerPick = request.SecondsPerPick,
            AutoPickSettings = request.AutoPickSettings?
                .Select(x => new DraftAutoPickEntry(x.TeamId, x.AutoPick))
                .ToList(),
        };
    }

    public DraftPlayerCommand ToCommand(int id, DraftPlayerRequest request)
    {
        return new DraftPlayerCommand(
            DraftId: id,
            PlayerId: request.PlayerId,
            TeamId: request.TeamId,
            OverallPick: request.OverallPick
        );
    }

    // ---------- CustomRanking ----------

    public IReadOnlyList<CustomRankingDto> ToDtoList(IReadOnlyList<CustomRanking> rankings)
    {
        return rankings
            .Select(ranking => new CustomRankingDto
            {
                Id = ranking.Id,
                Title = ranking.Title,
                Tournament = ToTournamentBriefDto(ranking.Tournament),
                PlayerCount = ranking.Players.Count,
                CreatedAt = ranking.CreatedAt,
            })
            .ToList();
    }

    public CustomRankingSingleDto? ToDto(CustomRanking? ranking)
    {
        if (ranking is null)
            return null;
        return new CustomRankingSingleDto
        {
            Id = ranking.Id,
            Title = ranking.Title,
            Tournament = ToTournamentBriefDto(ranking.Tournament),
            Players = ranking.Players
                .OrderBy(p => p.Rank)
                .Select(p => new CustomRankingPlayerDto
                {
                    Rank = p.Rank,
                    Player = ToPlayerBriefDto(p.Player),
                    GamesPlayed = p.GamesPlayed,
                    TotalPoints = p.TotalPoints,
                    PointsPerGame = p.PointsPerGame,
                })
                .ToList(),
        };
    }

    public CreateCustomRankingCommand ToCommand(CreateCustomRankingRequest request, ClaimsPrincipal user)
    {
        return new CreateCustomRankingCommand(
            TournamentId: request.TournamentId,
            Title: request.Title,
            OwnerAccountId: user.GetAccountId()
        );
    }

    public UpdateCustomRankingCommand ToCommand(UpdateCustomRankingRequest request)
    {
        return new UpdateCustomRankingCommand(
            Title: request.Title,
            OrderedPlayerIds: request.OrderedPlayerIds
        );
    }

    private DraftPickDto ToDraftPickDto(DraftPick draftPick)
    {
        return new DraftPickDto
        {
            DraftId = draftPick.DraftId,
            OverallPick = draftPick.OverallPick,
            Round = draftPick.Round,
            RoundPick = draftPick.RoundPick,
            Team = ToTeamBriefDto(draftPick.Team),
            Player = draftPick.Player is null ? null : ToPlayerBriefDto(draftPick.Player),
            ClockStartedAt = draftPick.ClockStartedAt,
        };
    }


    // ---------- Game ----------

    public GetGamesQuery ToQuery(GetGamesRequest request)
    {
        return new GetGamesQuery
        {
            TournamentId = request.TournamentId,
            TeamId = request.TeamId,
            Page = request.Page,
            Size = request.Size,
            SortBy = request.SortBy,
            Descending = request.Descending,
        };
    }

    public IPagedList<GameDto> ToDtoList(IPagedList<Game> games)
    {
        return games.ProjectTo(game => new GameDto
        {
            Id = game.Id,
            Tournament = ToTournamentBriefDto(game.Tournament),
            GameTime = game.GameTime,
            GameType = game.GameType,
            GameState = game.GameState,
            Venue = game.Venue,
            Rink = game.Rink,
            HomeTeam = ToTeamInGameDto(game, home: true),
            AwayTeam = ToTeamInGameDto(game, home: false),
        });
    }

    public GameSingleDto? ToDto(Game? game, IReadOnlyList<SkaterStat> homeStats, IReadOnlyList<SkaterStat> awayStats)
    {
        return game is null
            ? null
            : new GameSingleDto
            {
                Id = game.Id,
                Tournament = ToTournamentBriefDto(game.Tournament),
                GameTime = game.GameTime,
                GameType = game.GameType,
                GameState = game.GameState,
                Venue = game.Venue,
                Rink = game.Rink,
                HomeTeam = ToTeamInGameDto(game, home: true),
                AwayTeam = ToTeamInGameDto(game, home: false),
                Goals = game.Goals
                    .Select(ToGoalBriefDto)
                    .OrderBy(g => g.Period)
                    .ThenByDescending(g => g.TimeRemaining)
                    .ToList(),
                Penalties = game.Penalties
                    .Select(ToPenaltyBriefDto)
                    .OrderBy(penalty => penalty.Period)
                    .ThenByDescending(penalty => penalty.TimeRemaining)
                    .ToList(),
                Stars = GetGameStarDtos(game),
                Highlights = game.Highlights
                    .Select(ToGameHighlightDto)
                    .ToList(),
                PlayersToWatch = homeStats.Count == 0 || awayStats.Count == 0 ? [] :
                [
                    ToGameStatLeaderDto("Points",  homeStats.MaxBy(x => x.Points),  awayStats.MaxBy(x => x.Points),  x => x.Points),
                    ToGameStatLeaderDto("Goals",   homeStats.MaxBy(x => x.Goals),   awayStats.MaxBy(x => x.Goals),   x => x.Goals),
                    ToGameStatLeaderDto("Assists", homeStats.MaxBy(x => x.Assists), awayStats.MaxBy(x => x.Assists), x => x.Assists),
                ],
            };
    }

    private List<GameStarDto> GetGameStarDtos(Game game)
    {
        return game.Stars
            .Select(s =>
            {
                List<StatItem> stats;
                if (s.Player.Position == Core.Values.Position.Goalie)
                {
                    var goalsAgainst = game.Goals.Count(t => t.TeamId != s.Player.TeamId);
                    var gaa = (double)goalsAgainst;
                    stats =
                    [
                        new StatItem("GAA", $"{gaa:N2}"),
                    ];

                    if (goalsAgainst == 0)
                        stats = stats.Append(new StatItem("SO", "1")).ToList();
                }
                else
                {
                    var goals = game.Goals.Count(g => g.GoalPlayerId == s.Player.Id);
                    var assists = game.Goals.Count(g => g.Assist1PlayerId == s.Player.Id || g.Assist2PlayerId == s.Player.Id);
                    var points = goals + assists;
                    stats =
                    [
                        new StatItem("G", goals.ToString()),
                        new StatItem("A", assists.ToString()),
                        new StatItem("P", points.ToString())
                    ];
                }

                return new GameStarDto(
                    StarRank: s.StarRank,
                    Player: ToPlayerBriefDto(s.Player),
                    Stats: stats
                );
            })
            .OrderBy(gs => gs.StarRank)
            .ToList();
    }

    private GameHighlightDto ToGameHighlightDto(GameHighlight highlight)
    {
        var highlightUrls = _urlResolver.GetHighlightUrls(highlight.VideoId);
        return new GameHighlightDto(
            VideoUrl: highlightUrls?.VideoUrl ?? string.Empty,
            ThumbnailUrl: highlightUrls?.ThumbnailUrl ?? string.Empty,
            Title: highlight.Title,
            Description: highlight.Description,
            Player: highlight.Player is null ? null : ToPlayerBriefDto(highlight.Player)
        );
    }

    public UpdateGameStateCommand ToCommand(int gameId, UpdateGameStateRequest request)
        => new(gameId, request.State);

    public CreateGoalCommand ToCommand(int gameId, CreateGoalRequest request)
        => new(
            GameId: gameId,
            TeamId: request.TeamId,
            Period: request.Period,
            PeriodLabel: request.PeriodLabel,
            PeriodTimeRemaining: request.PeriodTimeRemaining,
            GoalPlayerId: request.GoalPlayerId,
            Assist1PlayerId: request.Assist1PlayerId,
            Assist2PlayerId: request.Assist2PlayerId,
            Notes: request.Notes
        );

    public UpdateGoalCommand ToCommand(int gameId, int goalId, UpdateGoalRequest request)
        => new(
            GameId: gameId,
            GoalId: goalId,
            TeamId: request.TeamId,
            Period: request.Period,
            PeriodLabel: request.PeriodLabel,
            PeriodTimeRemaining: request.PeriodTimeRemaining,
            GoalPlayerId: request.GoalPlayerId,
            Assist1PlayerId: request.Assist1PlayerId,
            Assist2PlayerId: request.Assist2PlayerId,
            Notes: request.Notes
        );

    public CreatePenaltyCommand ToCommand(int gameId, CreatePenaltyRequest request)
        => new(
            GameId: gameId,
            TeamId: request.TeamId,
            Period: request.Period,
            PeriodLabel: request.PeriodLabel,
            PeriodTimeRemaining: request.PeriodTimeRemaining,
            PlayerId: request.PlayerId,
            InfractionName: request.InfractionName,
            DurationMinutes: request.DurationMinutes,
            Notes: request.Notes
        );

    public UpdatePenaltyCommand ToCommand(int gameId, int penaltyId, UpdatePenaltyRequest request)
        => new(
            GameId: gameId,
            PenaltyId: penaltyId,
            TeamId: request.TeamId,
            Period: request.Period,
            PeriodLabel: request.PeriodLabel,
            PeriodTimeRemaining: request.PeriodTimeRemaining,
            PlayerId: request.PlayerId,
            InfractionName: request.InfractionName,
            DurationMinutes: request.DurationMinutes,
            Notes: request.Notes
        );

    public SetGameStarsCommand ToCommand(int gameId, SetGameStarsRequest request)
        => new(
            GameId: gameId,
            FirstStarPlayerId: request.FirstStarPlayerId,
            SecondStarPlayerId: request.SecondStarPlayerId,
            ThirdStarPlayerId: request.ThirdStarPlayerId
        );


    // ---------- GoalieStat ----------

    public GetGoalieStatsQuery ToQuery(GetGoalieStatsRequest request)
    {
        return new GetGoalieStatsQuery
        {
            TournamentId = request.TournamentId,
            TeamIds = request.TeamIds,
            GameId = request.GameId,
            Page = request.Page,
            Size = request.Size,
            SortBy = request.SortBy,
            Descending = request.Descending,
        };
    }

    public IPagedList<GoalieStatDto> ToDtoList(IPagedList<GoalieStat> goalies)
    {
        return goalies.ProjectTo(goalie => new GoalieStatDto
        {
            PlayerId = goalie.PlayerId,
            AccountId = goalie.AccountId,
            FirstName = goalie.FirstName,
            LastName = goalie.LastName,
            Position = goalie.Position,
            JerseyNumber = goalie.JerseyNumber,
            Birthday = goalie.Birthday,
            ProfilePicture = _urlResolver.GetFullUrl(goalie.ProfilePicture),
            TeamId = goalie.TeamId,
            TeamName = goalie.TeamName,
            TeamLogoUrl = _urlResolver.GetFullUrl(goalie.TeamLogoUrl),
            TeamAbbreviation = goalie.TeamAbbreviation,
            GamesPlayed = goalie.GamesPlayed,
            Goals = goalie.Goals,
            Assists = goalie.Assists,
            PenaltyMinutes = goalie.PenaltyMinutes,
            GoalsAgainst = goalie.GoalsAgainst,
            GoalsAgainstAverage = goalie.GoalsAgainstAverage,
            ShotsAgainst = goalie.ShotsAgainst,
            Saves = goalie.Saves,
            SavePercentage = goalie.SavePercentage,
            Shutouts = goalie.Shutouts,
            Wins = goalie.Wins
        });
    }


    // ---------- InfoGuide ----------

    public GetInfoGuidesQuery ToQuery(GetInfoGuidesRequest request)
    {
        return new GetInfoGuidesQuery
        {
            Page = request.Page,
            Size = request.Size,
            SortBy = request.SortBy,
            Descending = request.Descending,
        };
    }

    public IPagedList<InfoGuideDto> ToDtoList(IPagedList<InfoGuide> guides)
    {
        return guides.ProjectTo(guide => new InfoGuideDto
        {
            Id = guide.Id,
            Title = guide.Title,
            TournamentId = guide.TournamentId,
            Tournament = guide.Tournament == null ? null : ToTournamentBriefDto(guide.Tournament),
        });
    }

    public InfoGuideSingleDto? ToDto(InfoGuide? guide)
    {
        return guide is null
            ? null
            : new InfoGuideSingleDto
            {
                Id = guide.Id,
                Title = guide.Title,
                TournamentId = guide.TournamentId,
                Tournament = guide.Tournament == null ? null : ToTournamentBriefDto(guide.Tournament),
                MarkdownContent = guide.MarkdownContent
            };
    }


    // ---------- Player ----------

    public GetPlayersQuery ToQuery(GetPlayersRequest request)
    {
        return new GetPlayersQuery
        {
            TournamentId = request.TournamentId,
            TeamId = request.TeamId,
            Page = request.Page,
            Size = request.Size,
            SortBy = request.SortBy,
            Descending = request.Descending,
        };
    }

    public IPagedList<PlayerDto> ToDtoList(IPagedList<Player> players)
    {
        return players.ProjectTo(player => new PlayerDto
        {
            Id = player.Id,
            AccountId = player.AccountId,
            Position = player.Position,
            JerseyNumber = player.JerseyNumber,
            FirstName = player.Account!.FirstName,
            LastName = player.Account.LastName,
            Birthday = player.Account.Birthday,
            ProfilePicture = _urlResolver.GetFullUrl(player.Account.Avatar),
            BannerPicture = _urlResolver.GetFullUrl(player.Account.Banner),
            PreferredBeer = player.Account.PreferredBeer,
            Tournament = ToTournamentBriefDto(player.Tournament),
            Team = player.Team == null ? null : ToTeamBriefDto(player.Team),
        });
    }

    public PlayerSingleDto? ToDto(Player? player)
    {
        if (player is null)
            return null;

        return new PlayerSingleDto
        {
            Id = player.Id,
            AccountId = player.AccountId,
            Position = player.Position,
            JerseyNumber = player.JerseyNumber,
            FirstName = player.Account!.FirstName,
            LastName = player.Account.LastName,
            Birthday = player.Account.Birthday,
            ProfilePicture = _urlResolver.GetFullUrl(player.Account.Avatar),
            BannerPicture = _urlResolver.GetFullUrl(player.Account.Banner),
            PreferredBeer = player.Account.PreferredBeer,
            Height = player.Account.HeightFeet is null ? null : $"{player.Account.HeightFeet}'{player.Account.HeightInches}",
            Weight = player.Account.Weight,
            Tournament = ToTournamentBriefDto(player.Tournament),
            Team = player.Team == null ? null : ToTeamBriefDto(player.Team),
            TournamentStats = ToPlayerTournamentStatsDto(player),
            GameByGame = ToPlayerGameByGameDtos(player),
        };
    }


    // ---------- SkaterStat ----------

    public GetSkaterStatsQuery ToQuery(GetSkaterStatsRequest request)
    {
        return new GetSkaterStatsQuery
        {
            TournamentId = request.TournamentId,
            Position = request.Position,
            TeamIds = request.TeamIds,
            GameId = request.GameId,
            Page = request.Page,
            Size = request.Size,
            SortBy = request.SortBy,
            Descending = request.Descending
        };
    }

    public IPagedList<SkaterStatDto> ToDtoList(IPagedList<SkaterStat> skaters)
    {
        return skaters.ProjectTo(skater => new SkaterStatDto
        {
            PlayerId = skater.PlayerId,
            AccountId = skater.AccountId,
            FirstName = skater.FirstName,
            LastName = skater.LastName,
            Position = skater.Position,
            JerseyNumber = skater.JerseyNumber,
            Birthday = skater.Birthday,
            ProfilePicture = _urlResolver.GetFullUrl(skater.ProfilePicture),
            TeamId = skater.TeamId,
            TeamName = skater.TeamName,
            TeamLogoUrl = _urlResolver.GetFullUrl(skater.TeamLogoUrl),
            TeamAbbreviation = skater.TeamAbbreviation,
            GamesPlayed = skater.GamesPlayed,
            Goals = skater.Goals,
            Assists = skater.Assists,
            Points = skater.Points,
            PenaltyMinutes = skater.PenaltyMinutes
        });
    }


    // ---------- Stripe ----------

    public bool TryParseTournamentPaymentCommand(PaymentIntent paymentIntent, out ProcessTournamentPaymentIntentCommand command)
    {
        command = null!;
        if (paymentIntent.Metadata.TryGetValue("AccountId", out var accountIdStr)
            && paymentIntent.Metadata.TryGetValue("TournamentId", out var tournamentIdStr)
            && int.TryParse(accountIdStr, out var accountId)
            && int.TryParse(tournamentIdStr, out var tournamentId))
        {
            command = new ProcessTournamentPaymentIntentCommand(
                AccountId: accountId,
                TournamentId: tournamentId,
                PaymentId: paymentIntent.Id
            );
            return true;
        }

        return false;
    }

    public bool TryParseBracketChallengePaymentCommand(PaymentIntent paymentIntent,
        out ProcessBracketChallengePaymentIntentCommand command)
    {
        command = null!;
        if (paymentIntent.Metadata.TryGetValue("EventId", out var eventIdStr)
            && paymentIntent.Metadata.TryGetValue("Name", out var name)
            && paymentIntent.Metadata.TryGetValue("Email", out var email)
            && paymentIntent.Metadata.TryGetValue("AgreedToTOS", out var agreedToTOSStr)
            && int.TryParse(eventIdStr, out var eventId))
        {
            command = new ProcessBracketChallengePaymentIntentCommand(
                EventId: eventId,
                Name: name,
                Email: email,
                PaymentId: paymentIntent.Id,
                AgreedToTOS: agreedToTOSStr == "true"
            );
            return true;
        }

        return false;
    }


    // ---------- Team ----------

    public GetTeamsQuery ToQuery(GetTeamsRequest request)
    {
        return new GetTeamsQuery
        {
            TournamentId = request.TournamentId,
            Page = request.Page,
            Size = request.Size,
            SortBy = request.SortBy,
            Descending = request.Descending,
        };
    }

    public IPagedList<TeamDto> ToDtoList(IPagedList<Team> teams)
    {
        return teams.ProjectTo(team => new TeamDto
        {
            Id = team.Id,
            Name = team.Name,
            NameShort = team.NameShort,
            Abbreviation = team.Abbreviation,
            Tournament = ToTournamentBriefDto(team.Tournament),
            LogoUrl = _urlResolver.GetFullUrl(team.Logo),
            BannerUrl = _urlResolver.GetFullUrl(team.Banner),
            PrimaryColorHex = team.PrimaryColorHex,
            SecondaryColorHex = team.SecondaryColorHex,
            TertiaryColorHex = team.TertiaryColorHex,
            GoalSongUrl = _urlResolver.GetFullUrl(team.GoalSong),
            PenaltySongUrl = _urlResolver.GetFullUrl(team.PenaltySong),
            GmAccountId = team.GmAccountId,
            GmFirstName = team.GeneralManager?.FirstName,
            GmLastName = team.GeneralManager?.LastName,
            GmProfilePicture = _urlResolver.GetFullUrl(team.GeneralManager?.Avatar),
        });
    }

    public TeamSingleDto? ToDto(Team? team)
    {
        return team is null
            ? null
            : new TeamSingleDto
            {
                Id = team.Id,
                Name = team.Name,
                NameShort = team.NameShort,
                Abbreviation = team.Abbreviation,
                Tournament = ToTournamentBriefDto(team.Tournament),
                LogoUrl = _urlResolver.GetFullUrl(team.Logo),
                BannerUrl = _urlResolver.GetFullUrl(team.Banner),
                PrimaryColorHex = team.PrimaryColorHex,
                SecondaryColorHex = team.SecondaryColorHex,
                TertiaryColorHex = team.TertiaryColorHex,
                GoalSongUrl = _urlResolver.GetFullUrl(team.GoalSong),
                PenaltySongUrl = _urlResolver.GetFullUrl(team.PenaltySong),
                GmAccountId = team.GmAccountId,
                GmFirstName = team.GeneralManager?.FirstName,
                GmLastName = team.GeneralManager?.LastName,
                GmProfilePicture = _urlResolver.GetFullUrl(team.GeneralManager?.Avatar),
                Players = team.Players
                    .Select(ToPlayerBriefDto)
                    .ToList(),
            };
    }


    // ---------- Tournament ----------

    public GetTournamentsQuery ToQuery(GetTournamentsRequest request)
    {
        return new GetTournamentsQuery
        {
            RegistrationOpen = request.RegistrationOpen,
            Page = request.Page,
            Size = request.Size,
            SortBy = request.SortBy,
            Descending = request.Descending,
        };
    }

    public IPagedList<TournamentDto> ToDtoList(IPagedList<Tournament> tournaments)
    {
        return tournaments.ProjectTo(tournament => new TournamentDto
        {
            Id = tournament.Id,
            Name = tournament.Name,
            Logo = _urlResolver.GetFullUrl(tournament.Logo),
            StartDate = tournament.StartDate,
            EndDate = tournament.EndDate,
            WinningTeamId = tournament.WinningTeamId,
            IsActive = tournament.IsActive,
            IsRegistrationOpen = tournament.IsRegistrationOpen,
            IsPaymentOpen = tournament.IsPaymentOpen,
            SkaterLimit = tournament.SkaterLimit,
            GoalieLimit = tournament.GoalieLimit,
            Gallery = tournament.Gallery is null ? null : ToGalleryBriefDto(tournament.Gallery)
        });
    }

    public TournamentSingleDto? ToDto(Tournament? tournament)
    {
        return tournament is null
            ? null
            : new TournamentSingleDto
            {
                Id = tournament.Id,
                Name = tournament.Name,
                Logo = _urlResolver.GetFullUrl(tournament.Logo),
                StartDate = tournament.StartDate,
                EndDate = tournament.EndDate,
                WinningTeamId = tournament.WinningTeamId,
                IsActive = tournament.IsActive,
                IsRegistrationOpen = tournament.IsRegistrationOpen,
                IsPaymentOpen = tournament.IsPaymentOpen,
                SkaterLimit = tournament.SkaterLimit,
                GoalieLimit = tournament.GoalieLimit,
                InfoGuide = tournament.InfoGuide is null ? null : ToInfoGuideBriefDto(tournament.InfoGuide),
                Gallery = tournament.Gallery is null ? null : ToGalleryBriefDto(tournament.Gallery),
                Sponsors = tournament.Sponsors
                    .Select(sponsor => new TournamentSponsorDto
                    {
                        Name = sponsor.Name,
                        LogoUrl = _urlResolver.GetFullUrl(sponsor.Logo),
                        WebsiteUrl = sponsor.WebsiteUrl,
                    })
                    .ToList()
            };
    }

    public PlayerStatLeadersDto ToDto(string title, IEnumerable<SkaterStat> stats, Func<SkaterStat, double> selector, string? format = null)
    {
        return new PlayerStatLeadersDto
        {
            Title = title,
            Leaders = stats.Select(stat => ToPlayerStatDto(stat, selector, format))
        };
    }

    public GameStatLeaderDto ToGameStatLeaderDto(string title, SkaterStat? home, SkaterStat? away, Func<SkaterStat, double> selector, string? format = null)
    {
        return new GameStatLeaderDto(
            Title: title,
            HomeLeader: home is null ? null : ToGameStatLeaderPlayerDto(home, selector, format),
            AwayLeader: away is null ? null : ToGameStatLeaderPlayerDto(away, selector, format)
        );
    }

    private GameStatLeaderPlayerDto ToGameStatLeaderPlayerDto(SkaterStat stat, Func<SkaterStat, double> selector, string? format = null)
    {
        return new GameStatLeaderPlayerDto
        {
            PlayerId = stat.PlayerId,
            AccountId = stat.AccountId,
            FirstName = stat.FirstName,
            LastName = stat.LastName,
            Position = stat.Position,
            JerseyNumber = stat.JerseyNumber,
            ProfilePicture = _urlResolver.GetFullUrl(stat.ProfilePicture),
            StatValue = selector(stat),
            StatString = selector(stat).ToString(format)
        };
    }

    private PlayerStatDto ToPlayerStatDto(SkaterStat stat, Func<SkaterStat, double> selector, string? format = null)
    {
        return new PlayerStatDto
        {
            PlayerId = stat.PlayerId,
            AccountId = stat.AccountId,
            FirstName = stat.FirstName,
            LastName = stat.LastName,
            Position = stat.Position,
            JerseyNumber = stat.JerseyNumber,
            Birthday = stat.Birthday,
            ProfilePicture = _urlResolver.GetFullUrl(stat.ProfilePicture),
            TeamId = stat.TeamId,
            TeamName = stat.TeamName,
            TeamLogoUrl = _urlResolver.GetFullUrl(stat.TeamLogoUrl),
            TeamAbbreviation = stat.TeamAbbreviation,
            StatValue = selector(stat),
            StatString = selector(stat).ToString(format)
        };
    }

    public PlayerStatLeadersDto ToDto(string title, IEnumerable<GoalieStat> stats, Func<GoalieStat, double> selector, string? format = null)
    {
        return new PlayerStatLeadersDto
        {
            Title = title,
            Leaders = stats.Select(stat => new PlayerStatDto
            {
                PlayerId = stat.PlayerId,
                AccountId = stat.AccountId,
                FirstName = stat.FirstName,
                LastName = stat.LastName,
                Position = stat.Position,
                JerseyNumber = stat.JerseyNumber,
                Birthday = stat.Birthday,
                ProfilePicture = _urlResolver.GetFullUrl(stat.ProfilePicture),
                TeamId = stat.TeamId,
                TeamName = stat.TeamName,
                TeamLogoUrl = _urlResolver.GetFullUrl(stat.TeamLogoUrl),
                TeamAbbreviation = stat.TeamAbbreviation,
                StatValue = selector(stat),
                StatString = selector(stat).ToString(format)
            })
        };
    }


    // ---------- TournamentPayment ----------

    public TournamentPaymentIntentDto ToDto(TournamentPaymentIntent paymentIntent)
    {
        return new TournamentPaymentIntentDto(
            ClientSecret: paymentIntent.Secret,
            TotalAmount: paymentIntent.Amount,
            Currency: paymentIntent.Currency,
            Breakdown: paymentIntent.AmountBreakdown
        );
    }

    public CreateTournamentPaymentIntentCommand ToCommand(int tournamentId, int accountId, CreateTournamentPaymentIntentRequest request)
    {
        return new CreateTournamentPaymentIntentCommand(
            AccountId: accountId,
            TournamentId: tournamentId,
            Position: request.Position
        );
    }


    // ---------- TournamentRegistration ----------

    public TournamentRegistrationDto? ToDto(TournamentRegistration? registration)
    {
        return registration is null
            ? null
            : new TournamentRegistrationDto
            {
                CurrentStep = registration.CurrentStep,
                Payload = registration.Payload,
                IsComplete = registration.IsComplete,
            };
    }


    // ---------- Brief helpers ----------

    private DraftPickBriefDto? ToDraftPickBriefDto(DraftPick? draftPick)
    {
        return draftPick is null
            ? null
            : new DraftPickBriefDto
            {
                DraftId = draftPick.DraftId,
                OverallPick = draftPick.OverallPick,
                Round = draftPick.Round,
                RoundPick = draftPick.RoundPick,
                Team = ToTeamBriefDto(draftPick.Team),
            };
    }

    private GalleryBriefDto ToGalleryBriefDto(Gallery gallery)
    {
        return new GalleryBriefDto
        {
            Id = gallery.Id,
            Title = gallery.Title,
            Description = gallery.Description,
            Url = gallery.Source,
        };
    }

    private GoalBriefDto ToGoalBriefDto(Goal goal)
    {
        return new GoalBriefDto
        {
            Id = goal.Id,
            TimeRemaining = goal.PeriodTimeRemaining,
            Period = goal.Period,
            TeamId = goal.TeamId,
            Scorer = ToPlayerBriefDto(goal.Scorer),
            PrimaryAssist = goal.Assist1Player == null ? null : ToPlayerBriefDto(goal.Assist1Player),
            SecondaryAssist = goal.Assist2Player == null ? null : ToPlayerBriefDto(goal.Assist2Player),
        };
    }

    private InfoGuideBriefDto ToInfoGuideBriefDto(InfoGuide infoGuide)
    {
        return new InfoGuideBriefDto
        {
            Title = infoGuide.Title,
            MarkdownContent = infoGuide.MarkdownContent,
        };
    }

    private PenaltyBriefDto ToPenaltyBriefDto(Penalty penalty)
    {
        return new PenaltyBriefDto
        {
            Id = penalty.Id,
            TimeRemaining = penalty.PeriodTimeRemaining,
            Period = penalty.Period,
            TeamId = penalty.TeamId,
            Player = ToPlayerBriefDto(penalty.Player),
            Infraction = penalty.InfractionName,
            DurationMins = penalty.DurationMinutes
        };
    }

    private PlayerBriefDto ToPlayerBriefDto(Player player)
    {
        return new PlayerBriefDto
        {
            Id = player.Id,
            AccountId = player.AccountId,
            Position = player.Position,
            JerseyNumber = player.JerseyNumber,
            FirstName = player.Account.FirstName,
            LastName = player.Account.LastName,
            Birthday = player.Account.Birthday,
            ProfilePicture = _urlResolver.GetFullUrl(player.Account.Avatar),
            CaptaincyTag = player.Captaincy switch
            {
                Captaincy.Captain => 'C',
                Captaincy.Alternate => 'A',
                _ => null
            }
        };
    }

    private List<PlayerGameByGame> ToPlayerGameByGameDtos(Player player)
    {
        var gameByGames = player.Account.Players
            .SelectMany(p => (
                    (p.Team?.HomeGames ?? []).Select(g => new { Player = p, Game = g, IsHome = true, Opponent = g.AwayTeam })
                )
                .Concat(
                    (p.Team?.AwayGames ?? []).Select(g => new { Player = p, Game = g, IsHome = false, Opponent = g.HomeTeam })
                )
            );

        return gameByGames
            .Where(pg => pg.Game.GameState != GameState.Pending)
            .Select(pg => new PlayerGameByGame
            {
                Goals = pg.Game.Goals.Count(x => x.GoalPlayerId == pg.Player.Id),
                Assists = pg.Game.Goals.Count(x =>
                    x.Assist1PlayerId == pg.Player.Id || x.Assist2PlayerId == pg.Player.Id),
                PenaltyMinutes = pg.Game.Penalties.Where(x => x.PlayerId == pg.Player.Id).Sum(x => x.DurationMinutes),
                Win = pg.Game.Goals.Count(x => x.TeamId == pg.Player.TeamId) >
                      pg.Game.Goals.Count(x => x.TeamId != pg.Player.TeamId),
                Shutouts = pg.Game.Goals.All(x => x.TeamId == pg.Player.TeamId) ? 1 : 0,
                GoalsAgainst = pg.Game.Goals.Count(x => x.TeamId != pg.Player.TeamId),
                Tournament = ToTournamentBriefDto(pg.Game.Tournament),
                Game = new GameOfTeamDto
                {
                    Id = pg.Game.Id,
                    TournamentId = pg.Game.TournamentId,
                    TournamentName = pg.Game.Tournament.Name,
                    GameTime = pg.Game.GameTime,
                    GameType = pg.Game.GameType,
                    Venue = pg.Game.Venue,
                    Rink = pg.Game.Rink,
                    IsHome = pg.IsHome,
                    GoalsFor = pg.Game.Goals.Count(x => x.TeamId == pg.Player.TeamId),
                    GoalsAgainst = pg.Game.Goals.Count(x => x.TeamId != pg.Player.TeamId),
                    Opponent = pg.Opponent == null ? null : ToTeamBriefDto(pg.Opponent),
                },
                Team = pg.Player.Team == null ? null : ToTeamBriefDto(pg.Player.Team)
            })
            .ToList();
    }

    private List<PlayerTournamentStats> ToPlayerTournamentStatsDto(Player player)
    {
        return player.Account.Players.GroupBy(p => p.Tournament)
            .Select(g => new PlayerTournamentStats
            {
                GamesPlayed = g.Sum(x => x.SkaterGameLogs.Count + x.GoalieGameLogs.Count),
                Goals = g.Sum(x => x.Goals.Count),
                Assists = g.Sum(x => x.PrimaryAssists.Count + x.SecondaryAssists.Count),
                PenaltyMinutes = g.Sum(x => x.Penalties.Sum(p => p.DurationMinutes)),
                Wins = g.Sum(x => x.GoalieGameLogs.Sum(gl => gl.Wins)),
                Shutouts = g.Sum(x => x.GoalieGameLogs.Sum(gl => gl.Shutouts)),
                GoalsAgainstAverage = g
                    .SelectMany(x => x.GoalieGameLogs)
                    .Select(x => x.GoalsAgainst)
                    .DefaultIfEmpty(0)
                    .Average(),
                Tournament = ToTournamentBriefDto(g.Key),
                Team = g.First().Team == null ? null : ToTeamBriefDto(g.First().Team!),
            })
            .ToList();
    }

    private TeamBriefDto ToTeamBriefDto(Team team)
    {
        return new TeamBriefDto
        {
            Id = team.Id,
            Name = team.Name,
            NameShort = team.NameShort,
            Abbreviation = team.Abbreviation,
            Logo = _urlResolver.GetFullUrl(team.Logo),
            Banner = _urlResolver.GetFullUrl(team.Banner),
            PrimaryColorHex = team.PrimaryColorHex,
            SecondaryColorHex = team.SecondaryColorHex,
            TertiaryColorHex = team.TertiaryColorHex
        };
    }

    private TeamInGameDto? ToTeamInGameDto(Game game, bool home)
    {
        var team = home ? game.HomeTeam : game.AwayTeam;
        return team is null
        ? null
        : new TeamInGameDto
        {
            Id = team.Id,
            Name = team.Name,
            NameShort = team.NameShort,
            Abbreviation = team.Abbreviation,
            Logo = _urlResolver.GetFullUrl(team.Logo),
            Banner = _urlResolver.GetFullUrl(team.Banner),
            Goals = game.Goals.Count(g => g.TeamId == team.Id),
            PrimaryColorHex = team.PrimaryColorHex,
            SecondaryColorHex = team.SecondaryColorHex,
            TertiaryColorHex = team.TertiaryColorHex
        };
    }

    private TournamentBriefDto ToTournamentBriefDto(Tournament tournament)
    {
        return new TournamentBriefDto
        {
            Id = tournament.Id,
            Name = tournament.Name,
            StartDate = tournament.StartDate,
            EndDate = tournament.EndDate,
            WinningTeamId = tournament.WinningTeamId,
            IsActive = tournament.IsActive,
            IsRegistrationOpen = tournament.IsRegistrationOpen,
            Logo = _urlResolver.GetFullUrl(tournament.Logo),
        };
    }
}
