namespace BoltonCup.WebClient.Services;


public sealed record TournamentRegistrationContext(int TournamentId, int CurrentStep, bool IsComplete, TournamentRegistrationModel Model);