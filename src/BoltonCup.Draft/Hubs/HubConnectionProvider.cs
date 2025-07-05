using Microsoft.AspNetCore.SignalR.Client;

namespace BoltonCup.Draft.Hubs;

public class HubConnectionProvider : IAsyncDisposable
{
    private HubConnection _hubConnection;

    public HubConnectionProvider(HubConnection hubConnection)
    {
        _hubConnection = hubConnection;
    }

    public bool IsConnected => _hubConnection != null && _hubConnection.State == HubConnectionState.Connected;

    public async Task<HubConnection> GetHubConnection()
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
        {
            await _hubConnection.StartAsync();
        }

        return _hubConnection;
    }

    public async Task SendAsync(string methodName)
    {
        if (IsConnected)
        {
            await _hubConnection.SendAsync(methodName);
        }
    }    
    
    public async Task SendAsync(string methodName, object? arg1)
    {
        if (IsConnected)
        {
            await _hubConnection.SendAsync(methodName, arg1);
        }
    }

    public async Task SendAsync(string methodName, object? arg1, object? arg2)
    {
        if (IsConnected)
        {
            await _hubConnection.SendAsync(methodName, arg1, arg2);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}