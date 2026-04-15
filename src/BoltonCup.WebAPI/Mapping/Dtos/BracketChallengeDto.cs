namespace BoltonCup.WebAPI.Mapping;

public record BracketChallengeDto(
    int Id,
    string? Title,
    string? Link,
    decimal? Fee,
    bool IsOpen
);
