using System.Globalization;
using BoltonCup.Sdk;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BoltonCup.Timekeeper.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject]
    IBoltonCupApi BoltonCupApi { get; set; } = null!;

    [Inject]
    NavigationManager Navigation { get; set; } = null!;

    [Inject]
    ISnackbar Snackbar { get; set; } = null!;

    [SupplyParameterFromQuery(Name = "date")]
    string? DateParam { get; set; }

    bool _loading = true;
    GameDtoIPagedList? _gamesResult;
    List<GameDto> _games = [];
    DateTime? _selectedDate = DateTime.Today;
    DateTime? _lastFiltered;

    protected override async Task OnParametersSetAsync()
    {
        var date = DateTime.TryParseExact(DateParam, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var parsed) ? parsed.Date : DateTime.Today;
        _selectedDate = date;
        if (_lastFiltered == date)
        {
            return; // guard: replace:true re-enters this method
        }

        _lastFiltered = date;
        await LoadGamesAsync();
    }

    void OnDateChanged(DateTime? date)
    {
        var day = (date ?? DateTime.Today).Date;
        // null removes ?date= → bare "/" for today (clean URL)
        var value = day == DateTime.Today ? null : day.ToString("yyyy-MM-dd");
        Navigation.NavigateTo(
            Navigation.GetUriWithQueryParameter("date", value), replace: true);
    }

    async Task LoadGamesAsync()
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

    void SelectGame(GameDto game) => Navigation.NavigateTo($"/game/{game.Id}");

    static Color GetStateColor(GameState state) => state switch
    {
        GameState.InProgress => Color.Error,
        GameState.Completed => Color.Success,
        _ => Color.Default,
    };
}