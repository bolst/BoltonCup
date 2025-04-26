using BoltonCup.Shared.Data;

namespace BoltonCup.Draft.Data;

public class DraftServiceProvider
{
    private readonly IBCData _bcData;
    private const int DRAFT_ID = 2;

    public DraftServiceProvider(IBCData bcData)
    {
        _bcData = bcData;
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
            int order = currentPick.pick + 1;

            if (currentPick.round % 2 == 0)
            {
                order = 6 - currentPick.pick + 1;
            }
            
            var team = (await _bcData.GetTeamByDraftOrderAsync(DRAFT_ID, order))!;
            
            var pick = new BCDraftPick
            {
                draft_id = DRAFT_ID,
                pick = currentPick.pick == 6 ? 1 : currentPick.pick + 1,
                round = currentPick.pick == 6 ? (currentPick.round + 1) : currentPick.round,
            };
            
            return (team, pick);
        }
    }

    public async Task DraftPlayerAsync(PlayerProfile player)
    {
        var (team, pick) = await GetTeamWithCurrentPick();
        Console.WriteLine($"{team.name} is drafting {player.name}");
        
        // TODO:
        // 1. update database to indicate the drafted player is now on the team with current pick
        await _bcData.DraftPlayerAsync(player, team, pick);
        
        // 2. notify subscribers that a selection has been made (e.g., timer)
    }

    public async Task<IEnumerable<BCTeam>> GetTeamsInDraftAsync()
    {
        var teams = await _bcData.GetTeamsInTournamentAsync(DRAFT_ID);
        var draftOrder = await _bcData.GetDraftOrderAsync(DRAFT_ID);

        return teams.OrderBy(t => draftOrder.First(d => d.team_id == t.id).order);
    }

    public async Task<IEnumerable<object>> GetSelectedPlayersAsync()
    {
        throw new NotImplementedException();
    }
}