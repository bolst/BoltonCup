using BoltonCup.Sdk;
using BoltonCup.Timekeeper.Services;
using BoltonCup.Timekeeper.Services.Music;
using BoltonCup.Timekeeper.Components.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BoltonCup.Timekeeper.Components.Pages;

public partial class GameConsole : ComponentBase, IDisposable
{
    [Parameter]
    [EditorRequired]
    public required int GameId { get; set; }

    [Inject]
    TimekeeperStateService State { get; set; } = null!;

    [Inject]
    IDialogService DialogService { get; set; } = null!;

    [Inject]
    MusicPlayerService Player { get; set; } = null!;

    [Inject]
    ISnackbar Snackbar { get; set; } = null!;

    static readonly (int Value, string Label)[] PeriodOptions =
    [
        (1, "1st"), (2, "2nd"), (3, "3rd"), (4, "OT"),
    ];

    int? _loadedGameId;

    protected override void OnInitialized() => State.OnStateChanged += HandleStateChanged;

    protected override async Task OnParametersSetAsync()
    {
        if (GameId == _loadedGameId)
        {
            return;
        }

        _loadedGameId = GameId;
        await State.LoadGameAsync(GameId);
    }

    void HandleStateChanged() => InvokeAsync(StateHasChanged);

    async Task StartGameAsync()
    {
        var dialog = await DialogService.ShowAsync<StartGameDialog>("Start Game");
        var result = await dialog.Result;
        if (result is { Canceled: false, Data: bool includePlayerSongs })
        {
            await State.UpdateGameStateAsync(GameState.InProgress, includePlayerSongs);
        }
    }

    async Task EndGameAsync()
    {
        var confirmed = await DialogService.ShowMessageBoxAsync(
            "End Game",
            "Are you sure you want to end this game?",
            yesText: "End Game",
            cancelText: "Cancel");
        if (confirmed != true)
        {
            return;
        }

        // Play the winning team's win song as a one-shot the moment End Game is confirmed. A tie has
        // no winner, so nothing plays.
        var winner = State.HomeScore > State.AwayScore ? State.Game!.HomeTeam
            : State.AwayScore > State.HomeScore ? State.Game!.AwayTeam
            : null;
        if (State.AutoplayWinSong && winner is not null)
        {
            var songResult = await Player.PlayGoalSongAsync(
                winner.WinSongFileKey ?? "", winner.WinSongOffsetSeconds ?? 0, winner.WinSongTitle);
            var teamName = winner.NameShort ?? winner.Name ?? "team";
            switch (songResult)
            {
                case MusicPlayerService.GoalSongResult.NoSong:
                    Snackbar.Add($"No win song set for {teamName}.", Severity.Info);
                    break;
                case MusicPlayerService.GoalSongResult.Unresolved:
                    Snackbar.Add("Win song isn't downloaded and you're offline.", Severity.Warning);
                    break;
                case MusicPlayerService.GoalSongResult.Blocked:
                    Snackbar.Add("Browser blocked the win song from playing.", Severity.Warning);
                    break;
                case MusicPlayerService.GoalSongResult.NotReady:
                    Snackbar.Add("Music player isn't ready yet.", Severity.Warning);
                    break;
            }
        }

        await State.UpdateGameStateAsync(GameState.Completed);
    }

    async Task OpenGoalDialogAsync(bool isHome)
    {
        var team = isHome ? State.Game!.HomeTeam : State.Game!.AwayTeam;
        if (State.AutoplayGoalSong)
        {
            var songResult = await Player.PlayGoalSongAsync(
                team?.GoalSongFileKey ?? "", team?.GoalSongOffsetSeconds ?? 0, team?.GoalSongTitle);
            var teamName = team?.NameShort ?? team?.Name ?? "team";
            switch (songResult)
            {
                case MusicPlayerService.GoalSongResult.NoSong:
                    Snackbar.Add($"No goal song set for {teamName}.", Severity.Info);
                    break;
                case MusicPlayerService.GoalSongResult.Unresolved:
                    Snackbar.Add("Goal song isn't downloaded and you're offline.", Severity.Warning);
                    break;
                case MusicPlayerService.GoalSongResult.Blocked:
                    Snackbar.Add("Browser blocked the goal song from playing.", Severity.Warning);
                    break;
                case MusicPlayerService.GoalSongResult.NotReady:
                    Snackbar.Add("Music player isn't ready yet.", Severity.Warning);
                    break;
            }
        }

        var parameters = new DialogParameters<GoalDialog>
        {
            { x => x.HomeTeam, State.Game!.HomeTeam },
            { x => x.AwayTeam, State.Game!.AwayTeam },
            { x => x.HomePlayers, State.HomePlayers },
            { x => x.AwayPlayers, State.AwayPlayers },
            { x => x.CurrentPeriod, State.CurrentPeriod },
            { x => x.IsHome, isHome },
        };
        var dialog = await DialogService.ShowAsync<GoalDialog>("Record Goal", parameters);
        var result = await dialog.Result;
        if (result is { Canceled: false, Data: CreateGoalRequest request })
        {
            await State.AddGoalAsync(request);
        }
    }

    async Task OpenPenaltyDialogAsync(bool isHome)
    {
        var team = isHome ? State.Game!.HomeTeam : State.Game!.AwayTeam;
        if (State.AutoplayPenaltySong)
        {
            var songResult = await Player.PlayGoalSongAsync(
                team?.PenaltySongFileKey ?? "", team?.PenaltySongOffsetSeconds ?? 0, team?.PenaltySongTitle);
            var teamName = team?.NameShort ?? team?.Name ?? "team";
            switch (songResult)
            {
                case MusicPlayerService.GoalSongResult.NoSong:
                    Snackbar.Add($"No penalty song set for {teamName}.", Severity.Info);
                    break;
                case MusicPlayerService.GoalSongResult.Unresolved:
                    Snackbar.Add("Penalty song isn't downloaded and you're offline.", Severity.Warning);
                    break;
                case MusicPlayerService.GoalSongResult.Blocked:
                    Snackbar.Add("Browser blocked the penalty song from playing.", Severity.Warning);
                    break;
                case MusicPlayerService.GoalSongResult.NotReady:
                    Snackbar.Add("Music player isn't ready yet.", Severity.Warning);
                    break;
            }
        }

        var parameters = new DialogParameters<PenaltyDialog>
        {
            { x => x.HomeTeam, State.Game!.HomeTeam },
            { x => x.AwayTeam, State.Game!.AwayTeam },
            { x => x.HomePlayers, State.HomePlayers },
            { x => x.AwayPlayers, State.AwayPlayers },
            { x => x.CurrentPeriod, State.CurrentPeriod },
            { x => x.IsHome, isHome },
        };
        var dialog = await DialogService.ShowAsync<PenaltyDialog>("Record Penalty", parameters);
        var result = await dialog.Result;
        if (result is { Canceled: false, Data: CreatePenaltyRequest request })
        {
            await State.AddPenaltyAsync(request);
        }
    }

    async Task OpenStarsDialogAsync()
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

    async Task<bool> ConfirmDeletionAsync(string title)
    {
        var result = await DialogService.ShowMessageBoxAsync(title, "Are you sure you want to delete this?",
            yesText: "Delete", cancelText: "Cancel");
        return result == true;
    }

    async Task DeleteGoalAsync(int goalId)
    {
        if (await ConfirmDeletionAsync("Delete Goal"))
        {
            await State.DeleteGoalAsync(goalId);
        }
    }

    async Task DeletePenaltyAsync(int penaltyId)
    {
        if (await ConfirmDeletionAsync("Delete Penalty"))
        {
            await State.DeletePenaltyAsync(penaltyId);
        }
    }

    static Color GetStateColor(GameState state) => state switch
    {
        GameState.InProgress => Color.Error,
        GameState.Completed => Color.Success,
        _ => Color.Default,
    };

    public void Dispose() => State.OnStateChanged -= HandleStateChanged;
}