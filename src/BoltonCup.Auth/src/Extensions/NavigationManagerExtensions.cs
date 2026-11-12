using BoltonCup.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace BoltonCup.Auth.Extensions;

public static class NavigationManagerExtensions
{
    public static void NavigateWithReturnUrl(this NavigationManager navigation, string uri, bool forceLoad = false)
    {
        var returnUrl = navigation.GetRawReturnUrl();
        if (!string.IsNullOrEmpty(returnUrl))
        {
            uri += "?returnUrl=" + Uri.EscapeDataString(returnUrl);
        }

        navigation.NavigateTo(uri, forceLoad: forceLoad);
    }

    /// <summary>Returns the <c>returnUrl</c> query parameter when it points at a Bolton Cup site; otherwise <c>null</c>.</summary>
    public static string? GetReturnUrl(this NavigationManager navigation, string rootDomain, bool allowLocalhost) =>
        ReturnUrlPolicy.Sanitize(navigation.GetRawReturnUrl(), new Uri(navigation.BaseUri), rootDomain, allowLocalhost);

    static string? GetRawReturnUrl(this NavigationManager navigation)
    {
        var uriBuilder = new UriBuilder(navigation.Uri);
        var query = QueryHelpers.ParseQuery(uriBuilder.Query);
        return query.GetValueOrDefault("returnUrl");
    }
}