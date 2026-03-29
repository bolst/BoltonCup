using System.ComponentModel.DataAnnotations;
using static BoltonCup.Core.Values.Position;

namespace BoltonCup.WebAPI.Mapping;

public record CreateTournamentPaymentIntentRequest
{
    [AllowedValues(Forward, Defense, Goalie, ErrorMessage = $"Position must be one of '{Forward}', '{Defense}', '{Goalie}'")]
    public string Position { get; set; }
}