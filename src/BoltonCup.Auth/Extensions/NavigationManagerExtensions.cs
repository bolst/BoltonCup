using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace BoltonCup.Auth.Extensions;

public static class NavigationManagerExtensions
{
    public static void NavigateWithReturnUrl(this NavigationManager navigation, string uri, bool forceLoad = false)
    {
        var returnUrl = navigation.GetReturnUrl();
        if (!string.IsNullOrEmpty(returnUrl))
            uri += "?returnUrl=" + returnUrl;
        navigation.NavigateTo(uri, forceLoad: forceLoad);
    }
    
    public static string? GetReturnUrl(this NavigationManager navigation)
    {
        var uriBuilder = new UriBuilder(navigation.Uri);
        var query = QueryHelpers.ParseQuery(uriBuilder.Query);
        return query.GetValueOrDefault("returnUrl");
    }
}