using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using static BoltonCup.Infrastructure.Identity.BoltonCupRole;

namespace BoltonCup.WebAPI.Hubs;

[Authorize(Roles = Admin)]
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