using SpotifyAPI.Web;

namespace BoltonCup.Shared.Data;

public interface IBCData
{
    Task<IEnumerable<BCTeam>> GetTeams();
    Task<BCTeam?> GetTeamById(int id);
    Task<IEnumerable<BCGame>> GetSchedule();
    Task<IEnumerable<BCGame>> GetPlayerSchedule(int playerId);
    Task<BCGame?> GetGameById(int id);
    Task<IEnumerable<PlayerProfile>> GetRosterByTeamId(int teamId);
    Task<IEnumerable<PlayerProfile>> GetAllTournamentPlayersAsync(int tournamentId);
    Task<PlayerProfile?> GetPlayerProfileById(int playerId);
    Task<PlayerProfile?> GetUserTournamentPlayerProfileAsync(int accountId, int tournamentId);
    Task<IEnumerable<PlayerGameSummary>> GetPlayerGameByGame(int accountId, int? tournamentId = null);
    Task<IEnumerable<GoalieGameSummary>> GetGoalieGameByGame(int accountId, int? tournamentId = null);
    Task<IEnumerable<GameGoal>> GetGameGoalsByGameId(int gameId);
    Task<IEnumerable<GamePenalty>> GetGamePenaltiesByGameId(int gameId);
    Task<IEnumerable<PlayerStatLine>> GetPlayerStats(int tournamentId, int? teamId = null);
    Task<IEnumerable<GoalieStatLine>> GetGoalieStats(int tournamentId, int? teamId = null);
    Task<IEnumerable<PlayerProfilePicture>> GetPlayerProfilePictures();
    Task<string> SubmitRegistration(RegisterFormModel form);
    Task<IEnumerable<RegisterFormModel>> GetRegistrationsAsync();
    Task<RegisterFormModel?> GetRegistrationByEmailAsync(string email);
    Task<string> AdmitUserAsync(RegisterFormModel form);
    Task<string> RemoveAdmittedUserAsync(BCAccount account);
    Task<IEnumerable<BCAccount>> GetAccountsAsync();
    Task<BCAccount?> GetAccountByEmailAsync(string email);
    Task<BCAccount?> GetAccountByIdAsync(int accountId);
    Task UpdateAccountProfilePictureAsync(string email, string imagePath);
    Task<IEnumerable<BCTournament>> GetTournamentsAsync();
    Task<BCTournament?> GetTournamentByIdAsync(int id);
    Task<BCTournament?> GetCurrentTournamentAsync();
    Task SetUserAsPayedAsync(string email);
    Task ConfigPlayerProfileAsync(RegisterFormModel form, int tournamentId);
    Task<BCDraftPick?> GetMostRecentDraftPickAsync(int draftId);
    Task<BCTeam?> GetTeamByDraftOrderAsync(int draftId, int order);
    Task<IEnumerable<BCTeam>> GetTeamsInTournamentAsync(int tournamentId);
    Task<IEnumerable<PlayerProfile>> GetDraftAvailableTournamentPlayersAsync(int tournamentId);
    Task<IEnumerable<BCDraftOrder>> GetDraftOrderAsync(int draftId);
    Task DraftPlayerAsync(PlayerProfile player, BCTeam team, BCDraftPick draftPick);
    Task<IEnumerable<BCDraftPickDetail>> GetDraftPicksAsync(int draftId);
    Task ResetDraftAsync(int draftId);
    Task<IEnumerable<BCSponsor>> GetActiveSponsorsAsync();
    Task<IEnumerable<BCGame>> GetIncompleteGamesAsync();
    Task<BCAccount?> GetAccountByPCKeyAsync(Guid pckey);
    Task<PlayerProfile?> GetCurrentPlayerProfileByPCKeyAsync(Guid pckey);
    Task UpdateConfigDataAsync(BCAccount account);
    Task UpdatePlayerAvailabilityAsync(IEnumerable<BCAvailability> availabilities);
    Task<IEnumerable<BCAvailability>> GetPlayerAvailabilityAsync(int accountId, int tournamentId);
    Task PopulatePlayerAvailabilitiesAsync(int accountId);
    
    
    Task<BCRefreshToken?> GetRefreshToken(Guid localId);
    Task UpdateRefreshToken(Guid localId, string token);


    Task UpdatePlayerSongAsync(int accountId, FullTrack song);
    Task SetBCPlaylistSongsAsync(IEnumerable<FullTrack> songs);
    Task SetGamePlaylistAsync(int gameId, string playlistId);
    Task<IEnumerable<BCSong>> GetGameSongsAsync(int gameId);
    Task<int> GetSongOffsetAsync(string spotifyId);
    
    
    Task<IEnumerable<BCGame>> GetActiveGamesAsync();
    Task BeginRecordingGameAsync(int gameId);
    Task EndRecordingGameAsync(int gameId, bool complete = false);
}