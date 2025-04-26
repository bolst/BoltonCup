using Microsoft.AspNetCore.SignalR;

namespace BoltonCup.Draft.Hubs;

public class DraftHub : Hub
{
    public const string HubUrl = "/drafthub";

    public async Task PushDraftUpdate()
    {
        await Clients.All.SendAsync(Events.OnDraftUpdate);
    }

    public record Events
    {
        public const string OnDraftUpdate = "OnDraftUpdate";
    }
}