using BoltonCup.Draft.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace BoltonCup.Draft.Components.Shared;

public abstract class HubComponentBase : ComponentBase, IAsyncDisposable
{
    [Inject] 
    private NavigationManager Navigation { get; set; }
    
    protected HubConnection Hub { get; private set; }
    
    protected bool IsHubConnected => Hub != null && Hub.State == HubConnectionState.Connected;



    protected override async Task OnInitializedAsync()
    {
        Hub ??= new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri(DraftHub.HubUrl))
            .WithAutomaticReconnect()
            .Build();

        if (!IsHubConnected)
        {
            await Hub.StartAsync();
        }

        AddHubHandlers();
    }
    
    protected virtual void AddHubHandlers() { }


    public async ValueTask DisposeAsync() => await Hub.DisposeAsync();
}