using BoltonCup.Shared.Data;
using BoltonCup.Draft.Hubs;
using Microsoft.AspNetCore.SignalR.Client;

namespace BoltonCup.Draft.Data;

public class DraftServiceProvider
{
    private readonly IBCData _bcData;
    private readonly HubConnectionProvider _hub;
    
    private const int DRAFT_ID = 2;
    private const int PICKS_PER_ROUND = 6;

    public DraftServiceProvider(IBCData bcData, HubConnectionProvider hub)
    {
        _bcData = bcData;
        _hub = hub;
    }
    
    public int DraftId => DRAFT_ID;

    public async Task<(BCTeam, BCDraftPick)> GetTeamWithCurrentPick()
    {
        var currentPick = await _bcData.GetMostRecentDraftPickAsync(DRAFT_ID);

        if (currentPick is null)
        {
            int order = 1;

            var team = (await _bcData.GetTeamByDraftOrderAsync(DRAFT_ID, order))!;
            
            var pick = new BCDraftPick
            {
                draft_id = DRAFT_ID,
                pick = 1,
                round = 1,
            };
            
            return (team, pick);
        }
        else
        {
            int order = currentPick.pick >= PICKS_PER_ROUND ? 1 : currentPick.pick + 1;

            if (currentPick.round % 2 == 0)
            {
                order = PICKS_PER_ROUND - currentPick.pick + 1;
            }
            
            var team = (await _bcData.GetTeamByDraftOrderAsync(DRAFT_ID, order))!;
            
            var pick = new BCDraftPick
            {
                draft_id = DRAFT_ID,
                pick = currentPick.pick >= PICKS_PER_ROUND ? 1 : currentPick.pick + 1,
                round = currentPick.pick >= PICKS_PER_ROUND ? (currentPick.round + 1) : currentPick.round,
            };
            
            return (team, pick);
        }
    }

    public async Task DraftPlayerAsync(PlayerProfile player)
    {
        var (team, pick) = await GetTeamWithCurrentPick();
        Console.WriteLine($"{team.name} is drafting {player.name}");
        
        // 1. update database to indicate the drafted player is now on the team with current pick
        await _bcData.DraftPlayerAsync(player, team, pick);
        
        // 2. notify subscribers that a selection has been made (e.g., timer)
        await _hub.SendAsync(nameof(DraftHub.PushDraftUpdate));
    }

    public async Task<IEnumerable<BCTeam>> GetTeamsInDraftAsync()
    {
        var teams = await _bcData.GetTeamsInTournamentAsync(DRAFT_ID);
        var draftOrder = await _bcData.GetDraftOrderAsync(DRAFT_ID);

        return teams.OrderBy(t => draftOrder.First(d => d.team_id == t.id).order);
    }

    public async Task<IEnumerable<BCDraftPickDetail>> GetDraftedPlayersAsync()
    {
        return await _bcData.GetDraftPicksAsync(DRAFT_ID);
    }

    public async Task ResetDraftAsync()
    {
        await _bcData.ResetDraftAsync(DRAFT_ID);
        
        // notify subscribers
        await _hub.SendAsync(nameof(DraftHub.PushDraftUpdate));
    }
}