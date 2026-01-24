using BoltonCup.Core.Queries.Base;
using BoltonCup.Core.Values;
using FluentValidation;

namespace BoltonCup.Core.Queries;

public class GetSkaterGameLogsQueryValidator : PaginationQueryValidator<GetSkaterGameLogsQuery>
{
    public GetSkaterGameLogsQueryValidator()
    {
        var positions = new List<string>() { Position.Forward, Position.Defense };
        RuleFor(x => x.Position)
            .Must(x => positions.Contains(x!))
            .When(x => !string.IsNullOrEmpty(x.Position))
            .WithMessage("Position must be one of: " + string.Join(", ", positions));
        
        var homeOrAwayOptions = new List<string>() { HomeOrAway.Home, HomeOrAway.Away };
        RuleFor(x => x.HomeOrAway)
            .Must(x => homeOrAwayOptions.Contains(x!))
            .When(x => !string.IsNullOrEmpty(x.HomeOrAway))
            .WithMessage("HomeOrAway must be one of: " + string.Join(", ", homeOrAwayOptions));
    }
}