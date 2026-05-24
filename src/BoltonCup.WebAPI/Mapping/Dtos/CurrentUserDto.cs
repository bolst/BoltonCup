namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing the currently authenticated user.</summary>
/// <param name="UserId">The unique identifier of the user from the identity provider.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="Name">The user's display name.</param>
/// <param name="Roles">The list of roles assigned to the user.</param>
/// <param name="IsAuthenticated">Whether the user is currently authenticated.</param>
/// <param name="AccountId">The Bolton Cup account ID linked to this user, if any.</param>
/// <param name="TeamGmIds">The IDs of teams for which the user is a general manager.</param>
/// <param name="TournamentGmIds">The IDs of tournaments for which the user is a general manager.</param>
public record CurrentUserDto(
    string UserId,
    string Email,
    string Name,
    List<string> Roles,
    bool IsAuthenticated,
    int? AccountId,
    IEnumerable<int> TeamGmIds,
    IEnumerable<int> TournamentGmIds
);