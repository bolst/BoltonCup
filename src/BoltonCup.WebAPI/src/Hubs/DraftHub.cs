using Microsoft.AspNetCore.SignalR;

namespace BoltonCup.WebAPI.Hubs;

/// <summary>SignalR hub for real-time draft event broadcasting.</summary>
public class DraftHub : Hub
{
    /// <summary>Adds the caller to the SignalR group for the specified draft.</summary>
    public async Task JoinLiveDraft(int draftId)
    {
        var groupName = $"Draft_{draftId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    /// <summary>Removes the caller from the SignalR group for the specified draft.</summary>
    public async Task LeaveLiveDraft(int draftId)
    {
        var groupName = $"Draft_{draftId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}