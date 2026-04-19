namespace BoltonCup.Core.Commands;

public sealed record CreateDraftCommand( 
    int TournamentId,
    DraftType DraftType
);