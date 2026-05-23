using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public interface ITournamentRegistrationMapper
{
    TournamentRegistrationDto? ToDto(TournamentRegistration? registration);
}

public class TournamentRegistrationMapper : ITournamentRegistrationMapper
{
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
