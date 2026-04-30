using Microsoft.AspNetCore.SignalR;

namespace BoltonCup.WebAPI.Hubs;

public class DraftHub : Hub
{
    public async Task JoinLiveDraft(int draftId)
    {
        var groupName = $"Draft_{draftId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveLiveDraft(int draftId)
    {
        var groupName = $"Draft_{draftId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}