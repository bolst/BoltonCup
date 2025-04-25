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
}