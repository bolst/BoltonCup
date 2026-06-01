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
    private List<GameDto> _games = [];

    protected override async Task OnInitializedAsync()
    {
        var result = await BoltonCupApi.GetGamesAsync(size: 100);
        _games = result.Items
            .Where(g => g.GameTime.Date == DateTime.Today)
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
