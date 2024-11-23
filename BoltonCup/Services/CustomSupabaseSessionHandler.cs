using Blazored.LocalStorage;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace BoltonCup.Data;

public class CustomSupabaseSessionHandler : IGotrueSessionPersistence<Session>
{
    private readonly ILocalStorageService _localStorage;
    private const string SessionKey = "SUPABASE_SESSION";

    public CustomSupabaseSessionHandler(ILocalStorageService localStorage)
    {
        //logger.LogInformation("------------------- CONSTRUCTOR -------------------");
        _localStorage = localStorage;
    }

    public async void DestroySession()
    {
        //_logger.LogInformation("------------------- SessionDestroyer -------------------");
        try
        {
            await _localStorage.RemoveItemAsync(SessionKey);
        }
        catch  { }
    }

    public async void SaveSession(Session session)
    {
        //_logger.LogInformation("------------------- SessionPersistor -------------------");
        await _localStorage.SetItemAsync(SessionKey, session);
    }

    public Session? LoadSession()
    {
        //_logger.LogInformation("------------------- SessionRetriever -------------------");
        try
        {

            var session = _localStorage.GetItemAsync<Session>(SessionKey).Result;
            return session?.ExpiresAt() <= DateTime.Now ? null : session;
        }
        catch { return null; }
    }
}