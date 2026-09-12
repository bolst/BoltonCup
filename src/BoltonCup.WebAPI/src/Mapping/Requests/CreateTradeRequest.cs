namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to propose a player trade between two teams in a tournament.</summary>
public record CreateTradeRequest
{
    /// <summary>Gets the tournament the trade takes place in.</summary>
    public required int TournamentId { get; set; }
    /// <summary>Gets the proposing team's ID.</summary>
    public required int ProposingTeamId { get; set; }
    /// <summary>Gets the receiving team's ID.</summary>
    public required int ReceivingTeamId { get; set; }
    /// <summary>Gets the IDs of players the proposing team is sending.</summary>
    public IReadOnlyList<int> ProposingPlayerIds { get; set; } = [];
    /// <summary>Gets the IDs of players the receiving team is sending.</summary>
    public IReadOnlyList<int> ReceivingPlayerIds { get; set; } = [];
    /// <summary>Gets an optional note attached to the proposal.</summary>
    public string? Note { get; set; }
}