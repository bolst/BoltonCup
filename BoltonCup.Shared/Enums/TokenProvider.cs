using System.ComponentModel;

namespace BoltonCup.Shared.Data;

public enum TokenProvider
{
    [Description("spotify")]
    Spotify,
    
    [Description("supabase")]
    Supabase
}