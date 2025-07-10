using BoltonCup.Shared.Data;
using BoltonCup.Draft.Hubs;
using Microsoft.AspNetCore.SignalR.Client;

namespace BoltonCup.Draft.Data;

public class DraftServiceProvider
{
    private readonly IBCData _bcData;
    
    private BCDraft? _draft;
    private const int PICKS_PER_ROUND = 6;

    public DraftServiceProvider(IBCData bcData)
    {
        _bcData = bcData;
    }

    public async Task<BCDraft> GetDraftAsync()
    {
        if (_draft is not null) return _draft;
        
        var currentTournament = await _bcData.GetCurrentTournamentAsync() ?? throw new InvalidOperationException("No active tournament!");
        _draft = await _bcData.GetTournamentDraftAsync(currentTournament.tournament_id) ?? throw new InvalidOperationException("Draft not found.");

        return _draft;
    }

    public void InvalidateDraftCache() => _draft = null;

    
    public async Task<(BCTeam, BCDraftPick)> GetTeamWithCurrentPick()
    {
        var draft = await GetDraftAsync();
        
        var currentPick = await _bcData.GetMostRecentDraftPickAsync(draft.Id);

        if (currentPick is null)
        {
            int order = 1;

            var team = (await _bcData.GetTeamByDraftOrderAsync(draft.Id, order))!;
            
            var pick = new BCDraftPick
            {
                draft_id = draft.Id,
                pick = 1,
                round = 1,
            };
            
            return (team, pick);
        }
        else
        {
            int order = currentPick.pick >= PICKS_PER_ROUND ? 1 : currentPick.pick + 1;

            var pick = new BCDraftPick
            {
                draft_id = draft.Id,
                pick = currentPick.pick >= PICKS_PER_ROUND ? 1 : currentPick.pick + 1,
                round = currentPick.pick >= PICKS_PER_ROUND ? (currentPick.round + 1) : currentPick.round,
            };
            
            if (pick.round % 2 == 0)
            {
                order = PICKS_PER_ROUND - pick.pick + 1;
            }
            
            var team = (await _bcData.GetTeamByDraftOrderAsync(draft.Id, order))!;
            
            return (team, pick);
        }
    }

    public async Task DraftPlayerAsync(PlayerProfile player, HubConnection hub)
    {
        var (team, pick) = await GetTeamWithCurrentPick();
        // Console.WriteLine($"{team.name} is drafting {player.name}");
        
        // 1. update database to indicate the drafted player is now on the team with current pick
        await _bcData.DraftPlayerAsync(player, team, pick);
        
        // 2. notify subscribers that a selection has been made (e.g., timer)
        await hub.SendAsync(nameof(DraftHub.PushDraftUpdate));
    }

    public async Task<IEnumerable<BCTeam>> GetTeamsInDraftAsync()
    {
        var draft = await GetDraftAsync();
        
        var teams = await _bcData.GetTeamsInTournamentAsync(draft.Id);
        var draftOrder = await _bcData.GetDraftOrderAsync(draft.Id);

        return teams.OrderBy(t => draftOrder.First(d => d.team_id == t.id).order);
    }

    public async Task<IEnumerable<BCDraftPickDetail>> GetDraftedPlayersAsync()
    {
        var draft = await GetDraftAsync();
        return await _bcData.GetDraftPicksAsync(draft.Id);
    }

    public async Task ResetDraftAsync(HubConnection hub)
    {
        var draft = await GetDraftAsync();
        await _bcData.ResetDraftAsync(draft.Id);
        
        // notify subscribers
        await hub.SendAsync(nameof(DraftHub.PushDraftUpdate));
    }
}