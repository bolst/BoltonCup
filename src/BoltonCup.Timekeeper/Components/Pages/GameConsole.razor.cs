using BoltonCup.Sdk;
using BoltonCup.Timekeeper.Services;
using BoltonCup.Timekeeper.Components.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BoltonCup.Timekeeper.Components.Pages;

public partial class GameConsole : ComponentBase, IDisposable
{
    [Parameter]
    [EditorRequired]
    public required int GameId { get; set; }

    [Inject] private TimekeeperStateService State { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;

    private static readonly (int Value, string Label)[] PeriodOptions =
    [
        (1, "1st"), (2, "2nd"), (3, "3rd"), (4, "OT"),
    ];

    private int? _loadedGameId;

    protected override void OnInitialized()
    {
        State.OnStateChanged += HandleStateChanged;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (GameId == _loadedGameId) return;
        _loadedGameId = GameId;
        await State.LoadGameAsync(GameId);
    }

    private void HandleStateChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    private async Task StartGameAsync()
    {
        await State.UpdateGameStateAsync(GameState.InProgress);
    }

    private async Task EndGameAsync()
    {
        var confirmed = await DialogService.ShowMessageBoxAsync(
            "End Game",
            "Are you sure you want to end this game?",
            yesText: "End Game",
            cancelText: "Cancel");
        if (confirmed == true)
        {
            await State.UpdateGameStateAsync(GameState.Completed);
        }
    }

    private async Task OpenGoalDialogAsync()
    {
        var parameters = new DialogParameters<GoalDialog>
        {
            { x => x.HomeTeam, State.Game!.HomeTeam },
            { x => x.AwayTeam, State.Game!.AwayTeam },
            { x => x.HomePlayers, State.HomePlayers },
            { x => x.AwayPlayers, State.AwayPlayers },
            { x => x.CurrentPeriod, State.CurrentPeriod },
        };
        var dialog = await DialogService.ShowAsync<GoalDialog>("Record Goal", parameters);
        var result = await dialog.Result;
        if (result is { Canceled: false, Data: CreateGoalRequest request })
        {
            await State.AddGoalAsync(request);
        }
    }

    private async Task OpenPenaltyDialogAsync()
    {
        var parameters = new DialogParameters<PenaltyDialog>
        {
            { x => x.HomeTeam, State.Game!.HomeTeam },
            { x => x.AwayTeam, State.Game!.AwayTeam },
            { x => x.HomePlayers, State.HomePlayers },
            { x => x.AwayPlayers, State.AwayPlayers },
            { x => x.CurrentPeriod, State.CurrentPeriod },
        };
        var dialog = await DialogService.ShowAsync<PenaltyDialog>("Record Penalty", parameters);
        var result = await dialog.Result;
        if (result is { Canceled: false, Data: CreatePenaltyRequest request })
        {
            await State.AddPenaltyAsync(request);
        }
    }

    private async Task OpenStarsDialogAsync()
    {
        var parameters = new DialogParameters<GameStarsDialog>
        {
            { x => x.AllPlayers, State.AllPlayers.ToList() },
            { x => x.ExistingStars, State.Game!.Stars },
        };
        var dialog = await DialogService.ShowAsync<GameStarsDialog>("Game Stars", parameters);
        var result = await dialog.Result;
        if (result is { Canceled: false, Data: SetGameStarsRequest request })
        {
            await State.SetStarsAsync(request);
        }
    }

    private async Task<bool> ConfirmDeletionAsync(string title)
    {
        var result = await DialogService.ShowMessageBoxAsync(title, "Are you sure you want to delete this?",
            yesText: "Delete", cancelText: "Cancel");
        return result == true;
    }

    private async Task DeleteGoalAsync(int goalId)
    {
        if (await ConfirmDeletionAsync("Delete Goal"))
        {
            await State.DeleteGoalAsync(goalId);
        }
    }

    private async Task DeletePenaltyAsync(int penaltyId)
    {
        if (await ConfirmDeletionAsync("Delete Penalty"))
        {
            await State.DeletePenaltyAsync(penaltyId);
        }
    }

    private static Color GetStateColor(GameState state) => state switch
    {
        GameState.InProgress => Color.Error,
        GameState.Completed => Color.Success,
        _ => Color.Default,
    };

    public void Dispose()
    {
        State.OnStateChanged -= HandleStateChanged;
    }
}
