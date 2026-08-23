using BoltonCup.Common.Imaging;
using BoltonCup.Core;
using BoltonCup.Core.Exceptions;
using BoltonCup.Core.Values;
using BoltonCup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BoltonCup.Admin.Imaging;

public sealed class TeamRosterImageGenerator(
    IDbContextFactory<BoltonCupDbContext> _dbContextFactory,
    IRosterImageRenderer _renderer,
    IStorageService _storageService,
    IGeneratedImageService _generatedImageService,
    IAssetUrlResolver _assetUrlResolver,
    IHttpClientFactory _httpClientFactory,
    IMemoryCache _cache) : ITeamRosterImageGenerator
{
    // The template font lives in R2 under this fixed key (upload the .ttf there).
    const string FontStorageKey = "static/fonts/roster.ttf";
    const string FontCacheKey = "roster-font-ttf";
    const string ContentType = "image/png";

    // Live preview renders at base resolution for speed; the saved export is supersampled.
    const float PreviewScale = 1f;
    const float SaveScale = 3f;

    public async Task<TeamRosterOptions> GetRosterAsync(int teamId, CancellationToken cancellationToken = default)
    {
        var team = await LoadTeamAsync(teamId, cancellationToken);

        static IReadOnlyList<RosterPlayerOption> Options(IReadOnlyList<Player> players) => players
            .Select(p => new RosterPlayerOption(p.Id,
                $"{p.Account.FirstName} {p.Account.LastName}".Trim(), p.JerseyNumber))
            .ToList();

        return new TeamRosterOptions(
            Options(OrderRoster(team.Players, Position.Forward, null)),
            Options(OrderRoster(team.Players, Position.Defense, null)),
            Options(OrderRoster(team.Players, Position.Goalie, null)));
    }

    public async Task<byte[]> RenderAsync(int teamId, RosterColorway colorway, RosterOrdering? ordering = null,
        CancellationToken cancellationToken = default)
    {
        var model = await BuildModelAsync(teamId, colorway, ordering, cancellationToken);
        return _renderer.Render(model, PreviewScale);
    }

    public async Task<GeneratedImageResult> GenerateAsync(int teamId, RosterColorway colorway,
        RosterOrdering? ordering = null, CancellationToken cancellationToken = default)
    {
        var model = await BuildModelAsync(teamId, colorway, ordering, cancellationToken);
        var png = _renderer.Render(model, SaveScale);

        var storageKey = $"media/generated/{Guid.NewGuid():N}.png";
        using (var stream = new MemoryStream(png))
        {
            await _storageService.PutAssetAsync(stream, storageKey, ContentType, cancellationToken);
        }

        await _generatedImageService.CreateAsync(storageKey, TeamRosterTemplate.TemplateKey, model.TeamName, ContentType,
            cancellationToken);

        return new GeneratedImageResult(png, storageKey, ContentType);
    }

    async Task<Team> LoadTeamAsync(int teamId, CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Teams
                   .AsNoTracking()
                   .Include(t => t.Players).ThenInclude(p => p.Account)
                   .FirstOrDefaultAsync(t => t.Id == teamId, cancellationToken)
               ?? throw new EntityNotFoundException(nameof(Team), teamId);
    }

    async Task<RosterImageModel> BuildModelAsync(int teamId, RosterColorway colorway,
        RosterOrdering? ordering, CancellationToken cancellationToken)
    {
        var team = await LoadTeamAsync(teamId, cancellationToken);

        var fontBytes = await GetFontAsync(cancellationToken);
        var logoBytes = await GetLogoAsync(team.Logo, cancellationToken);

        return new RosterImageModel
        {
            TeamName = team.Name,
            PrimaryHex = team.PrimaryColorHex,
            SecondaryHex = team.SecondaryColorHex,
            TertiaryHex = team.TertiaryColorHex,
            LogoPng = logoBytes,
            FontTtf = fontBytes,
            Colorway = colorway,
            Forwards = await MapCellsAsync(team.Players, Position.Forward, ordering?.Forwards, logoBytes, cancellationToken),
            Defense = await MapCellsAsync(team.Players, Position.Defense, ordering?.Defense, logoBytes, cancellationToken),
            Goalies = await MapCellsAsync(team.Players, Position.Goalie, ordering?.Goalies, logoBytes, cancellationToken),
        };
    }

    // Filters players to one position and applies the desired order. When an explicit order is
    // given, listed players lead in that order and any unlisted players follow in the default sort.
    static IReadOnlyList<Player> OrderRoster(IEnumerable<Player> players, string position,
        IReadOnlyList<int>? order)
    {
        var filtered = players
            .Where(p => string.Equals(p.Position, position, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (order is { Count: > 0 })
        {
            var rank = new Dictionary<int, int>();
            for (var i = 0; i < order.Count; i++)
            {
                rank[order[i]] = i;
            }
            return filtered
                .OrderBy(p => rank.TryGetValue(p.Id, out var r) ? r : int.MaxValue)
                .ThenBy(p => p.JerseyNumber ?? int.MaxValue)
                .ThenBy(p => p.Account.LastName)
                .ToList();
        }

        return filtered
            .OrderBy(p => p.JerseyNumber ?? int.MaxValue)
            .ThenBy(p => p.Account.LastName)
            .ToList();
    }

    // Default hometown when a player has none set, matching the original static template value.
    const string DefaultHometown = "WINDSOR, ON";

    async Task<IReadOnlyList<RosterPlayerCell>> MapCellsAsync(IEnumerable<Player> players, string position,
        IReadOnlyList<int>? order, byte[]? teamLogo, CancellationToken cancellationToken)
    {
        var roster = OrderRoster(players, position, order);

        var cells = new List<RosterPlayerCell>(roster.Count);
        foreach (var p in roster)
        {
            // Fall back to the current team logo when the player has no previous-team logo set.
            var previousTeamLogo = await GetLogoAsync(p.Account.PreviousTeamLogo, cancellationToken) ?? teamLogo;
            cells.Add(new RosterPlayerCell
            {
                JerseyNumber = p.JerseyNumber,
                FirstName = p.Account.FirstName,
                LastName = p.Account.LastName,
                BirthYear = p.Account.Birthday.Year,
                Captaincy = p.Captaincy switch
                {
                    Core.Captaincy.Captain => 'C',
                    Core.Captaincy.Alternate => 'A',
                    _ => null,
                },
                Hometown = string.IsNullOrWhiteSpace(p.Account.Hometown) ? DefaultHometown : p.Account.Hometown,
                PreviousTeamLogoPng = previousTeamLogo,
            });
        }
        return cells;
    }

    async Task<byte[]> GetFontAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(FontCacheKey, out byte[]? cached) && cached is not null)
        {
            return cached;
        }

        var bytes = await DownloadAssetAsync(FontStorageKey, cancellationToken)
                    ?? throw new InvalidOperationException(
                        $"Roster font not found in storage at '{FontStorageKey}'. Upload the template .ttf there.");

        _cache.Set(FontCacheKey, bytes, TimeSpan.FromHours(6));
        return bytes;
    }

    // Cache the logo so repeated live-preview renders don't re-fetch it from storage on every change.
    async Task<byte[]?> GetLogoAsync(string? key, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(key))
        {
            return null;
        }
        if (_cache.TryGetValue($"logo:{key}", out byte[]? cached))
        {
            return cached;
        }

        var bytes = await DownloadAssetAsync(key, cancellationToken);
        _cache.Set($"logo:{key}", bytes, TimeSpan.FromMinutes(15));
        return bytes;
    }

    async Task<byte[]?> DownloadAssetAsync(string? key, CancellationToken cancellationToken)
    {
        var url = _assetUrlResolver.GetFullUrl(key);
        if (string.IsNullOrEmpty(url))
        {
            return null;
        }

        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}