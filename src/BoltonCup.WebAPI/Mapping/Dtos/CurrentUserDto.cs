namespace BoltonCup.WebAPI.Mapping;

public record CurrentUserDto(
    string UserId,
    string Email,
    string Name,
    List<string> Roles,
    bool IsAuthenticated,
    int? AccountId
);