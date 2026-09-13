using System.ComponentModel.DataAnnotations;
using static BoltonCup.Core.Values.Position;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to create a Stripe payment intent for tournament registration.</summary>
public record CreateTournamentPaymentIntentRequest
{
    /// <summary>Gets or sets the player's position (Forward, Defense, or Goalie).</summary>
    [AllowedValues(Forward, Defense, Goalie, ErrorMessage = $"Position must be one of '{Forward}', '{Defense}', '{Goalie}'")]
    public required string Position { get; set; }
}