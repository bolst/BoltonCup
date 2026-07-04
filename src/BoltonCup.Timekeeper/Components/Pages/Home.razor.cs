using System.Globalization;
using BoltonCup.Sdk;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BoltonCup.Timekeeper.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject]
    private IBoltonCupApi BoltonCupApi { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [SupplyParameterFromQuery(Name = "date")]
    private string? DateParam { get; set; }

    private bool _loading = true;
    private GameDtoIPagedList? _gamesResult;
    private List<GameDto> _games = [];
    private DateTime? _selectedDate = DateTime.Today;
    private DateTime? _lastFiltered;

    protected override async Task OnParametersSetAsync()
    {
        var date = DateTime.TryParseExact(DateParam, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var parsed) ? parsed.Date : DateTime.Today;
        _selectedDate = date;
        if (_lastFiltered == date) return; // guard: replace:true re-enters this method
        _lastFiltered = date;
        await LoadGamesAsync();
    }

    private void OnDateChanged(DateTime? date)
    {
        var day = (date ?? DateTime.Today).Date;
        // null removes ?date= → bare "/" for today (clean URL)
        var value = day == DateTime.Today ? null : day.ToString("yyyy-MM-dd");
        Navigation.NavigateTo(
            Navigation.GetUriWithQueryParameter("date", value), replace: true);
    }

    private async Task LoadGamesAsync()
    {
        _loading = true;
        _gamesResult ??= await BoltonCupApi.GetGamesAsync(size: 100);
        var filterDate = _selectedDate?.Date ?? DateTime.Today;
        _games = _gamesResult.Items
            .Where(g => g.GameTime.Date == filterDate)
            .OrderBy(g => g.GameTime)
            .ToList();
        _loading = false;
    }

    private void SelectGame(GameDto game)
    {
        Navigation.NavigateTo($"/game/{game.Id}");
    }

    private static Color GetStateColor(GameState state) => state switch
    {
        GameState.InProgress => Color.Error,
        GameState.Completed => Color.Success,
        _ => Color.Default,
    };
}
