using BoltonCup.Core;
using Microsoft.AspNetCore.Components;

namespace BoltonCup.WebClient.Services;

/// <summary>
/// Builds page URLs of the form <c>/{entity}/{id}/{slug}</c>. Pages load by ID alone; the slug only makes the URL
/// readable, so it is omitted when the text has nothing usable.
/// </summary>
public static class PageUrls
{
    public static string Player(int id, string? fullName) => WithSlug($"/players/{id}", fullName);

    public static string TeamRoster(int id, string? name) => $"{WithSlug($"/teams/{id}", name)}/roster";

    public static string Game(int id, string? awayAbbreviation, string? homeAbbreviation)
        => string.IsNullOrWhiteSpace(awayAbbreviation) || string.IsNullOrWhiteSpace(homeAbbreviation)
            ? $"/games/{id}"
            : WithSlug($"/games/{id}", $"{awayAbbreviation} vs {homeAbbreviation}");

    public static string TournamentRegister(int id, string? name) => Tournament(id, name, "register");

    public static string TournamentInformation(int id, string? name) => Tournament(id, name, "information");

    public static string TournamentPlayerInfo(int id, string? name) => Tournament(id, name, "player-info");

    public static string TournamentTradeHub(int id, string? name) => Tournament(id, name, "trade-hub");

    public static string BracketChallengeRegister(int id, string? title)
        => $"{WithSlug($"/bracket-challenges/{id}", title)}/register";

    /// <summary>
    /// Replaces the current URL with <paramref name="canonicalPath"/> when its path differs, keeping the query
    /// string and fragment. The browser history keeps no entry for the old URL.
    /// </summary>
    public static void RedirectToCanonical(this NavigationManager navigation, string canonicalPath)
    {
        var relative = "/" + navigation.ToBaseRelativePath(navigation.Uri);
        var suffixStart = relative.IndexOfAny(['?', '#']);
        var path = suffixStart < 0 ? relative : relative[..suffixStart];
        if (path == canonicalPath)
        {
            return;
        }

        var suffix = suffixStart < 0 ? string.Empty : relative[suffixStart..];
        navigation.NavigateTo(canonicalPath + suffix, replace: true);
    }

    static string Tournament(int id, string? name, string page) => $"{WithSlug($"/tournaments/{id}", name)}/{page}";

    static string WithSlug(string path, string? text)
    {
        var slug = SlugGenerator.Generate(text, fallback: string.Empty);
        return slug.Length == 0 ? path : $"{path}/{slug}";
    }
}