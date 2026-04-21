using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using static BoltonCup.Shared.HubEvents.DraftLobby;
using static BoltonCup.Infrastructure.Identity.BoltonCupRole;

namespace BoltonCup.WebAPI.Hubs;

[Authorize(Roles = Admin)]
public class DraftLobbyHub : Hub
{
    private static readonly ConcurrentDictionary<string, int> _activeUsers = new();

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;

        if (!string.IsNullOrEmpty(userId))
        {
            await Clients.Others.SendAsync(UserJoined, userId);
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;

        if (!string.IsNullOrEmpty(userId) && _activeUsers.TryGetValue(userId, out int count))
        {
            if (count <= 1)
            {
                _activeUsers.TryRemove(userId, out _);
                await Clients.Others.SendAsync(UserLeft, userId);
            }
            else
            {
                _activeUsers[userId] = count - 1;
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public Task<IEnumerable<string>> GetActiveUsers()
    {
        return Task.FromResult(_activeUsers.Keys.AsEnumerable());
    }
}