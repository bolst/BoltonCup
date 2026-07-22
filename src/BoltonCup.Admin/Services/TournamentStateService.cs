using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace BoltonCup.Admin.Services;

/// <summary>
/// Holds the admin-wide "current tournament" scope for a Blazor circuit, keeping the URL
/// (<c>?tournament=&lt;id&gt;</c>, shareable), browser localStorage (default across reloads/tabs),
/// and in-memory state in sync. Resolution precedence: URL query param → localStorage →
/// active tournament → first tournament.
/// </summary>
public sealed class TournamentStateService : IDisposable
{
    private const string StorageKey = "bc.admin.currentTournamentId";
    private const string QueryParam = "tournament";

    private readonly IDbContextFactory<BoltonCupDbContext> _dbFactory;
    private readonly IJSRuntime _js;
    private readonly NavigationManager _navigation;

    private Tournament? _current;
    private IReadOnlyList<Tournament>? _tournaments;

    public TournamentStateService(
        IDbContextFactory<BoltonCupDbContext> dbFactory,
        IJSRuntime js,
        NavigationManager navigation)
    {
        _dbFactory = dbFactory;
        _js = js;
        _navigation = navigation;
        _navigation.LocationChanged += OnLocationChanged;
    }

    /// <summary>Raised when the current tournament changes; subscribers should marshal to the UI thread.</summary>
    public event Action? Changed;

    public async Task<IReadOnlyList<Tournament>> GetTournamentsAsync(CancellationToken cancellationToken = default)
    {
        if (_tournaments is not null)
        {
            return _tournaments;
        }

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        _tournaments = await db.Tournaments
            .AsNoTracking()
            .OrderByDescending(t => t.StartDate ?? DateTime.MinValue)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);
        return _tournaments;
    }

    public async Task<Tournament?> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        if (_current is not null)
        {
            return _current;
        }

        var tournaments = await GetTournamentsAsync(cancellationToken);

        // 1. URL query param (shareable, authoritative).
        if (ReadUrlId() is int urlId
            && tournaments.FirstOrDefault(t => t.Id == urlId) is { } fromUrl)
        {
            _current = fromUrl;
            await WriteStoredIdAsync(urlId, cancellationToken);
            return _current;
        }

        // 2. Persisted default from a previous session.
        if (await ReadStoredIdAsync(cancellationToken) is int storedId
            && tournaments.FirstOrDefault(t => t.Id == storedId) is { } fromStore)
        {
            _current = fromStore;
            return _current;
        }

        // 3/4. Active tournament, else the first available.
        _current = tournaments.FirstOrDefault(t => t.IsActive) ?? tournaments.FirstOrDefault();
        return _current;
    }

    public async Task SetCurrentAsync(Tournament tournament, CancellationToken cancellationToken = default)
    {
        if (_current?.Id == tournament.Id)
        {
            return;
        }

        _current = tournament;
        await WriteStoredIdAsync(tournament.Id, cancellationToken);

        var url = _navigation.GetUriWithQueryParameter(QueryParam, (int?)tournament.Id);
        _navigation.NavigateTo(url);

        Changed?.Invoke();
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _ = HandleLocationChangedAsync();
    }

    private async Task HandleLocationChangedAsync()
    {
        if (ReadUrlId() is int id)
        {
            // A pasted/shared link (or manual URL edit) can switch scope mid-circuit.
            if (_current?.Id != id)
            {
                var tournaments = await GetTournamentsAsync();
                if (tournaments.FirstOrDefault(t => t.Id == id) is { } tournament)
                {
                    _current = tournament;
                    await WriteStoredIdAsync(id, CancellationToken.None);
                    Changed?.Invoke();
                }
            }
            return;
        }

        // Plain nav-menu hrefs drop the query param; re-apply it so the scope stays shareable.
        if (_current is not null)
        {
            var url = _navigation.GetUriWithQueryParameter(QueryParam, (int?)_current.Id);
            _navigation.NavigateTo(url, forceLoad: false, replace: true);
        }
    }

    private int? ReadUrlId()
    {
        var uri = _navigation.ToAbsoluteUri(_navigation.Uri);
        var query = QueryHelpers.ParseQuery(uri.Query);
        if (query.TryGetValue(QueryParam, out var values)
            && int.TryParse(values.FirstOrDefault(), out var id))
        {
            return id;
        }
        return null;
    }

    private async Task<int?> ReadStoredIdAsync(CancellationToken cancellationToken)
    {
        try
        {
            var raw = await _js.InvokeAsync<string?>("localStorage.getItem", cancellationToken, StorageKey);
            return int.TryParse(raw, out var id) ? id : null;
        }
        catch (Exception e) when (e is InvalidOperationException or JSException)
        {
            // JS interop is unavailable during prerender; fall back to non-persisted resolution.
            return null;
        }
    }

    private async Task WriteStoredIdAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", cancellationToken, StorageKey, id.ToString());
        }
        catch (Exception e) when (e is InvalidOperationException or JSException)
        {
            // Ignore during prerender; the value is re-applied after render.
        }
    }

    public void Dispose()
    {
        _navigation.LocationChanged -= OnLocationChanged;
    }
}
