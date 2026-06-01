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

    private bool _loading = true;
    private GameDtoIPagedList? _gamesResult;
    private List<GameDto> _games = [];
    private DateTime? _selectedDate = DateTime.Today;

    protected override async Task OnInitializedAsync()
    {
        await LoadGamesAsync();
    }

    private async Task OnDateChanged(DateTime? date)
    {
        _selectedDate = date;
        await LoadGamesAsync();
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
