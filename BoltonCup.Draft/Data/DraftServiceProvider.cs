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

    public async Task<BCTeam> GetTeamWithCurrentPick()
    {
        var currentPick = await _bcData.GetMostRecentDraftPickAsync(DRAFT_ID);

        if (currentPick is null)
        {
            int order = 1;
            
            return (await _bcData.GetTeamByDraftOrderAsync(DRAFT_ID, order))!;
        }
        else
        {
            int round = currentPick.round;
            int pick = currentPick.pick;

            int order = pick;

            if (round % 2 == 0)
            {
                order = 6 - pick + 1;
            }
            
            
            return (await _bcData.GetTeamByDraftOrderAsync(DRAFT_ID, order))!;
        }
    }

    public async Task DraftPlayerAsync(PlayerProfile player)
    {
        var team = await GetTeamWithCurrentPick();
        Console.WriteLine($"{team.name} is drafting {player.name}");
        
        // TODO:
        // 1. update database to indicate the drafted player is now on the team with current pick
        // 2. notify subscribers that a selection has been made (e.g., timer)
    }

    public async Task<IEnumerable<BCTeam>> GetTeamsInDraftAsync()
    {
        var teams = await _bcData.GetTeamsInTournamentAsync(DRAFT_ID);
        var draftOrder = await _bcData.GetDraftOrderAsync(DRAFT_ID);

        return teams.OrderBy(t => draftOrder.First(d => d.team_id == t.id).order);
    }

    public async Task<IEnumerable<PlayerProfile>> GetTeamRosterAsync(BCTeam team)
    {
        return await _bcData.GetRosterByTeamId(team.id);
    }
}