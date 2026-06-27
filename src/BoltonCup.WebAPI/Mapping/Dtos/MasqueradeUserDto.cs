namespace BoltonCup.WebAPI.Mapping;

/// <summary>A user an admin can masquerade as, returned by the masquerade search.</summary>
/// <param name="UserId">The identity user ID to masquerade as.</param>
/// <param name="AccountId">The linked Bolton Cup account ID, if any.</param>
/// <param name="Name">The user's display name, or their email if no account exists.</param>
/// <param name="Email">The user's email address.</param>
public record MasqueradeUserDto(string UserId, int? AccountId, string Name, string Email);
