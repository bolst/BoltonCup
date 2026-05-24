using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Maps <see cref="TournamentRegistration"/> entities to DTOs.</summary>
public interface ITournamentRegistrationMapper
{
    /// <summary>Maps a <see cref="TournamentRegistration"/> to a <see cref="TournamentRegistrationDto"/>, or returns <see langword="null"/> if the registration is null.</summary>
    TournamentRegistrationDto? ToDto(TournamentRegistration? registration);
}

/// <summary>Maps <see cref="TournamentRegistration"/> entities to DTOs.</summary>
public class TournamentRegistrationMapper : ITournamentRegistrationMapper
{
    /// <inheritdoc/>
    public TournamentRegistrationDto? ToDto(TournamentRegistration? registration)
    {
        return registration is null
            ? null
            : new TournamentRegistrationDto 
            { 
                CurrentStep = registration.CurrentStep,
                Payload = registration.Payload,
                IsComplete = registration.IsComplete,
            };
    }
}
