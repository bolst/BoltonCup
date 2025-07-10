using Microsoft.AspNetCore.SignalR;

namespace BoltonCup.Draft.Hubs;

public class DraftHub : Hub
{
    public const string HubUrl = "/drafthub";

    public async Task PushDraftUpdate()
    {
        await Clients.All.SendAsync(Events.OnDraftUpdate);
    }

    public async Task PushDraftStateChange()
    {
        await Clients.All.SendAsync(Events.OnDraftStateChange);
    }

    public async Task PushTimerStateChange(string jsCommand)
    {
        await Clients.Others.SendAsync(Events.OnTimerStateChange, jsCommand);
    }

    public record Events
    {
        public const string OnDraftUpdate = "OnDraftUpdate";
        public const string OnDraftStateChange = "OnDraftStateChange";
        public const string OnTimerStateChange = "OnTimerStateChange";
    }
}