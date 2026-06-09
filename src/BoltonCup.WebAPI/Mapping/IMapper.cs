using System.Security.Claims;
using BoltonCup.Core;
using BoltonCup.Core.BracketChallenge;
using BoltonCup.Core.Commands;
using Stripe;
using Account = BoltonCup.Core.Account;
using Event = BoltonCup.Core.BracketChallenge.Event;

namespace BoltonCup.WebAPI.Mapping;

public interface IMapper
{
    // Account
    AccountDto? ToDto(Account? account, ClaimsPrincipal claims);
    ICollection<AccountTournamentDto> ToAccountTournamentDtoList(Account? account);
    CreateAccountCommand ToCommand(CompleteUserAccountRequest request, ClaimsPrincipal claims);
    UpdateAccountCommand ToCommand(UpdateAccountRequest request, ClaimsPrincipal claims);

    // User
    CurrentUserDto? ToDto(ClaimsPrincipal claims);

    // BracketChallenge
    GetBracketChallengesQuery ToQuery(GetBracketChallengesRequest request);
    IPagedList<BracketChallengeDto> ToDtoList(IPagedList<Event> bracketChallenges);
    BracketChallengeSingleDto? ToDto(Event? bracketChallenge);
    BracketChallengePaymentIntentDto ToDto(BracketChallengePaymentIntent paymentIntent);
    CreateBracketChallengePaymentIntentCommand ToCommand(int bracketChallengeId, CreateBracketChallengePaymentIntentRequest request);

    // Draft
    IPagedList<DraftDto> ToDtoList(IPagedList<Draft> drafts);
    IPagedList<DraftPickDto> ToDtoList(IPagedList<DraftPick> draftPicks);
    IPagedList<DraftRankingDto> ToDtoList(IPagedList<PlayerDraftRanking> rankings, IReadOnlySet<int> favouritePlayerIds);
    DraftSingleDto? ToDto(Draft? draft, bool isAuthorized, bool canManage);
    DraftPickSingleDto? ToDto(DraftPick? draftPick);
    DraftUpdateEventDto ToDto(CurrentDraftState draftState, bool isAuthorized, bool canManage);
    DraftPickMadeEventDto ToDto(CurrentDraftStateWithPick draftState);
    GetDraftsQuery ToQuery(GetDraftsRequest request, ClaimsPrincipal user);
    CreateDraftCommand ToCommand(CreateDraftRequest request, ClaimsPrincipal user);
    UpdateDraftCommand ToCommand(UpdateDraftRequest request);
    DraftPlayerCommand ToCommand(int id, DraftPlayerRequest request);
    SetPlayerPoolCommand ToCommand(SetPlayerPoolRequest request);

    // CustomRanking
    IReadOnlyList<CustomRankingDto> ToDtoList(IReadOnlyList<CustomRanking> rankings);
    CustomRankingSingleDto? ToDto(CustomRanking? ranking);
    CreateCustomRankingCommand ToCommand(CreateCustomRankingRequest request, ClaimsPrincipal user);
    UpdateCustomRankingCommand ToCommand(UpdateCustomRankingRequest request);

    // Game
    GetGamesQuery ToQuery(GetGamesRequest request);
    IPagedList<GameDto> ToDtoList(IPagedList<Game> games);
    GameSingleDto? ToDto(Game? game, IReadOnlyList<SkaterStat> homeStats, IReadOnlyList<SkaterStat> awayStats);

    // GoalieStat
    GetGoalieStatsQuery ToQuery(GetGoalieStatsRequest request);
    IPagedList<GoalieStatDto> ToDtoList(IPagedList<GoalieStat> goalies);

    // InfoGuide
    GetInfoGuidesQuery ToQuery(GetInfoGuidesRequest request);
    IPagedList<InfoGuideDto> ToDtoList(IPagedList<InfoGuide> guides);
    InfoGuideSingleDto? ToDto(InfoGuide? guide);

    // Player
    GetPlayersQuery ToQuery(GetPlayersRequest request);
    IPagedList<PlayerDto> ToDtoList(IPagedList<Player> players);
    PlayerSingleDto? ToDto(Player? player);

    // SkaterStat
    GetSkaterStatsQuery ToQuery(GetSkaterStatsRequest request);
    IPagedList<SkaterStatDto> ToDtoList(IPagedList<SkaterStat> skaters);

    // Stripe
    bool TryParseTournamentPaymentCommand(PaymentIntent paymentIntent, out ProcessTournamentPaymentIntentCommand command);
    bool TryParseBracketChallengePaymentCommand(PaymentIntent paymentIntent, out ProcessBracketChallengePaymentIntentCommand command);

    // Team
    GetTeamsQuery ToQuery(GetTeamsRequest request);
    IPagedList<TeamDto> ToDtoList(IPagedList<Team> teams);
    TeamSingleDto? ToDto(Team? team);

    // Tournament
    GetTournamentsQuery ToQuery(GetTournamentsRequest request);
    IPagedList<TournamentDto> ToDtoList(IPagedList<Tournament> tournaments);
    TournamentSingleDto? ToDto(Tournament? tournament);
    PlayerStatLeadersDto ToDto(string title, IEnumerable<SkaterStat> stats, Func<SkaterStat, double> selector, string? format = null);
    PlayerStatLeadersDto ToDto(string title, IEnumerable<GoalieStat> stats, Func<GoalieStat, double> selector, string? format = null);
    GameStatLeaderDto ToGameStatLeaderDto(string title, SkaterStat? home, SkaterStat? away, Func<SkaterStat, double> selector, string? format = null);

    // TournamentPayment
    TournamentPaymentIntentDto ToDto(TournamentPaymentIntent paymentIntent);
    CreateTournamentPaymentIntentCommand ToCommand(int tournamentId, int accountId, CreateTournamentPaymentIntentRequest request);

    // TournamentRegistration
    TournamentRegistrationDto? ToDto(TournamentRegistration? registration);
}
