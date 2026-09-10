using BoltonCup.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;

namespace BoltonCup.Admin.Components.Shared;

/// <summary>
/// A recipient-field style tag editor: type to pick a target type, Tab into it, then search for the
/// target itself. Enter at the first stage commits a free-text label instead.
/// </summary>
public partial class TagInput<TTag> : ComponentBase, IDisposable
    where TTag : EntityTag, new()
{
    enum Stage
    {
        SelectingType,
        SelectingTarget,
    }

    const int DebounceMs = 200;

    Stage _stage = Stage.SelectingType;
    TagTargetType? _pendingType;
    string _text = string.Empty;
    IReadOnlyList<Option> _options = [];
    int _highlight;
    bool _open;
    bool _busy;
    CancellationTokenSource? _searchCts;
    ElementReference _input;

    /// <summary>Display text for tags added this session, whose navigations were never loaded.</summary>
    readonly Dictionary<int, string> _addedLabels = [];

    int _boundSubjectId = -1;
    bool _disposed;

    /// <summary>Index in _options where matching labels begin, or -1 when there are none.</summary>
    int _labelSectionStart = -1;

    [Parameter]
    [EditorRequired]
    public ITaggable<TTag> Subject { get; set; } = null!;

    [Inject] public ITagService<TTag> TagService { get; set; } = null!;
    [Inject] public ITagTargetSearchService TargetSearch { get; set; } = null!;
    [Inject] public IJSRuntime JS { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Not just on firstRender: a row saved from new renders its input later, and the
        // component instance is reused rather than recreated. Re-attaching is free.
        if (CanTag && _input.Context is not null)
        {
            await JS.InvokeVoidAsync("tagInput.attach", _input);
        }
    }

    /// <summary>Closing on blur is safe because the dropdown swallows mousedown, so clicks never blur.</summary>
    void OnBlur() => Close();

    /// <summary>An option in the dropdown: a target type at stage one, a target entity at stage two.</summary>
    sealed record Option(string Display, TagTargetType? Type, int? TargetId);

    bool CanTag => Subject.Id != 0;

    /// <summary>
    /// Tab must only be swallowed while it has something to commit, otherwise it stops moving
    /// focus out of the field. Bound at render time, which is fine: the dropdown always opens
    /// on a keystroke before Tab can be pressed.
    /// </summary>
    bool SuppressDefaultKey => _open && _options.Count > 0;

    static string Prefix(TagTargetType type) => type.ToString().ToLowerInvariant();

    static Color ChipColor(TagTargetType type) => type switch
    {
        TagTargetType.Game => Color.Info,
        TagTargetType.Player => Color.Success,
        TagTargetType.Team => Color.Warning,
        TagTargetType.Tournament => Color.Tertiary,
        TagTargetType.Label => Color.Default,
        _ => Color.Default,
    };

    static Color ChipColor(EntityTag tag)
        => TagTargets.GetTargetType(tag) is { } type ? ChipColor(type) : Color.Default;

    /// <summary>Grouped by target type, then alphabetically, so chips stay in a stable order.</summary>
    IEnumerable<TTag> OrderedTags() => Subject.Tags
        .OrderBy(t => TagTargets.GetTargetType(t) is { } type ? (int)type : int.MaxValue)
        .ThenBy(ChipLabel, StringComparer.OrdinalIgnoreCase);

    protected override void OnParametersSet()
    {
        // MudBlazor keys the main row but not the child row hosting this component, so on a grid
        // reload a live instance can be handed a different subject. Rebind resets everything.
        if (!CanTag || Subject.Id != _boundSubjectId)
        {
            _boundSubjectId = Subject.Id;
            Reset();
        }
    }

    async Task OnTextChanged(ChangeEventArgs e)
    {
        _text = e.Value?.ToString() ?? string.Empty;
        await RefreshOptionsAsync();
    }

    async Task OnKeyDown(KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case "Tab":
                if (e.ShiftKey)
                {
                    break;
                }

                await CommitHighlightedAsync();
                break;

            case "Enter":
            case "NumpadEnter":
                if (_stage == Stage.SelectingType)
                {
                    await CommitLabelAsync();
                }
                else
                {
                    await CommitHighlightedAsync();
                }
                break;

            case "Escape":
                if (_stage == Stage.SelectingTarget)
                {
                    await BackToTypeStageAsync();
                }
                else
                {
                    _text = string.Empty;
                    Close();
                }
                break;

            case "Backspace":
                // Auto-repeat would otherwise delete a chip per key event, ~30 a second.
                if (_text.Length > 0 || e.Repeat)
                {
                    break;
                }

                if (_stage == Stage.SelectingTarget)
                {
                    await BackToTypeStageAsync();
                }
                else if (OrderedTags().LastOrDefault() is { } last)
                {
                    await RemoveAsync(last);
                }
                break;

            case "ArrowDown":
                Move(1);
                break;

            case "ArrowUp":
                Move(-1);
                break;
        }
    }

    void Move(int delta)
    {
        if (_options.Count == 0)
        {
            return;
        }

        _open = true;
        _highlight = (_highlight + delta + _options.Count) % _options.Count;
    }

    async Task RefreshOptionsAsync()
    {
        // Cancel only; the in-flight search owns disposal, since disposing a token another
        // task is still awaiting surfaces as ObjectDisposedException instead of cancellation.
        _searchCts?.Cancel();
        _highlight = 0;
        _labelSectionStart = -1;

        if (_stage == Stage.SelectingType)
        {
            // Types render immediately; matching labels are appended when the search returns.
            var types = RankTypes(_text);
            _options = types;
            _open = _text.Length > 0 && types.Count > 0;

            if (_text.Length == 0)
            {
                Close();
                return;
            }

            var labels = await SearchAsync(TagTargetType.Label, _text);
            if (labels is null)
            {
                return;
            }

            _options = [.. types, .. labels];
            _labelSectionStart = labels.Count > 0 ? types.Count : -1;
            _open = _options.Count > 0;
        }
        else
        {
            // Stale options must go before the await: otherwise Tab during the debounce commits
            // a result belonging to the previous prefix.
            _options = [];
            _open = false;

            var results = await SearchAsync(_pendingType!.Value, _text);
            if (results is null)
            {
                return;
            }

            _options = results;
            _open = _options.Count > 0;
        }

        if (!_disposed)
        {
            StateHasChanged();
        }
    }

    /// <summary>Debounced search for one target type. Null means superseded or failed.</summary>
    async Task<List<Option>?> SearchAsync(TagTargetType type, string term)
    {
        var cts = new CancellationTokenSource();
        _searchCts = cts;
        _busy = true;

        try
        {
            await Task.Delay(DebounceMs, cts.Token);

            var exclude = Subject.Tags
                .Where(t => TagTargets.GetTargetType(t) == type)
                .Select(t => TagTargets.GetTargetId(t, type)!.Value)
                .ToList();

            var results = await TargetSearch.SearchAsync(type, term, exclude, cts.Token);
            return results.Select(r => new Option(r.Display, type, r.Id)).ToList();
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception e)
        {
            Snackbar.Add($"Could not search {Prefix(type)}s: {e.Message}", Severity.Error);
            return null;
        }
        finally
        {
            if (ReferenceEquals(_searchCts, cts))
            {
                _busy = false;
                _searchCts = null;
            }

            cts.Dispose();
        }
    }

    Option? HighlightedOption()
        => _open && _highlight >= 0 && _highlight < _options.Count ? _options[_highlight] : null;

    Option? FirstLabelOption()
        => _labelSectionStart >= 0 && _labelSectionStart < _options.Count ? _options[_labelSectionStart] : null;

    /// <summary>Prefix matches first, then substring matches, alphabetical within each band.</summary>
    static List<Option> RankTypes(string text) => TagTargets.Selectable
        .Select(t => (Type: t, Name: Prefix(t)))
        .Select(x => (x.Type, x.Name, Rank: Rank(x.Name, text)))
        .Where(x => x.Rank < 2)
        .OrderBy(x => x.Rank)
        .ThenBy(x => x.Name, StringComparer.Ordinal)
        .Select(x => new Option(x.Name, x.Type, null))
        .ToList();

    static int Rank(string name, string text)
    {
        if (text.Length == 0 || name.StartsWith(text, StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }
        if (name.Contains(text, StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }
        return 2;
    }

    async Task CommitHighlightedAsync()
    {
        if (!_open || _highlight >= _options.Count)
        {
            return;
        }

        await SelectAsync(_options[_highlight]);
    }

    async Task SelectAsync(Option option)
    {
        if (option.TargetId is null)
        {
            // Stage one: adopt the chosen type and start searching within it.
            _stage = Stage.SelectingTarget;
            _pendingType = option.Type;
            _text = string.Empty;
            await RefreshOptionsAsync();
            await FocusAsync();
            return;
        }

        await AddAsync(option.Type!.Value, option.TargetId.Value, option.Display);
        await BackToTypeStageAsync();
    }

    async Task CommitLabelAsync()
    {
        if (!CanTag)
        {
            return;
        }

        // Enter never picks an entity type. It takes the highlighted label if the user arrowed
        // onto one, otherwise the best-matching label, otherwise it creates one from the text.
        var chosen = HighlightedOption() is { Type: TagTargetType.Label, TargetId: not null } highlighted
            ? highlighted
            : FirstLabelOption();

        if (chosen is not null)
        {
            await AddAsync(TagTargetType.Label, chosen.TargetId!.Value, chosen.Display);
            _text = string.Empty;
            Close();
            await FocusAsync();
            return;
        }

        var name = _text.Trim();
        if (name.Length == 0)
        {
            return;
        }

        TTag? tag = null;
        if (!await TryAsync(async () => tag = await TagService.AddLabelTagAsync(Subject.Id, name), "Could not add label")
            || tag is null)
        {
            return;
        }

        if (Subject.Tags.All(t => t.Id != tag.Id))
        {
            Subject.Tags.Add(tag);
        }

        _text = string.Empty;
        Close();
        await FocusAsync();
    }

    async Task<bool> TryAsync(Func<Task> operation, string whatFailed)
    {
        try
        {
            await operation();
            return true;
        }
        catch (Exception e)
        {
            Snackbar.Add($"{whatFailed}: {e.Message}", Severity.Error);
            return false;
        }
    }

    async Task AddAsync(TagTargetType type, int targetId, string display)
    {
        if (!CanTag)
        {
            return;
        }

        TTag? tag = null;
        if (!await TryAsync(async () => tag = await TagService.AddTagAsync(Subject.Id, type, targetId), "Could not add tag")
            || tag is null)
        {
            return;
        }

        // The new tag has no loaded navigation, so remember the display text the option
        // already carried rather than re-reading the entity just to render one chip.
        if (Subject.Tags.All(t => t.Id != tag.Id))
        {
            _addedLabels[tag.Id] = display;
            Subject.Tags.Add(tag);
        }
    }

    async Task RemoveAsync(TTag tag)
    {
        if (!await TryAsync(() => TagService.RemoveTagAsync(tag.Id), "Could not remove tag"))
        {
            return;
        }

        Subject.Tags.Remove(tag);
        _addedLabels.Remove(tag.Id);
        await FocusAsync();
    }

    async Task BackToTypeStageAsync()
    {
        _stage = Stage.SelectingType;
        _pendingType = null;
        _text = string.Empty;
        await RefreshOptionsAsync();
        await FocusAsync();
    }

    void Close()
    {
        _open = false;
        _options = [];
        _highlight = 0;
        _busy = false;
        _labelSectionStart = -1;
    }

    void Reset()
    {
        _searchCts?.Cancel();
        Close();
        _stage = Stage.SelectingType;
        _pendingType = null;
        _text = string.Empty;
        _addedLabels.Clear();
    }

    public void Dispose()
    {
        _disposed = true;
        _searchCts?.Cancel();
    }

    async Task FocusAsync()
    {
        try
        {
            await _input.FocusAsync();
        }
        catch (Exception)
        {
            // the element can be gone mid-render; focus is a nicety, not worth failing over.
        }
    }

    string ChipLabel(EntityTag tag)
    {
        var type = TagTargets.GetTargetType(tag);
        if (type is null)
        {
            return "?";
        }

        if (_addedLabels.TryGetValue(tag.Id, out var added))
        {
            return $"{Prefix(type.Value)}: {added}";
        }

        var target = TagTargets.GetTargetEntity(tag, type.Value);
        return $"{Prefix(type.Value)}: {target?.ToString() ?? $"#{TagTargets.GetTargetId(tag, type.Value)}"}";
    }
}
