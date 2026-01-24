using BoltonCup.Core.Queries.Base;
using BoltonCup.Core.Values;
using FluentValidation;

namespace BoltonCup.Core.Queries;

public class GetGoalieGameLogsQueryValidator : PaginationQueryValidator<GetGoalieGameLogsQuery>
{
    public GetGoalieGameLogsQueryValidator()
    {
        var homeOrAwayOptions = new List<string>() { HomeOrAway.Home, HomeOrAway.Away };
        RuleFor(x => x.HomeOrAway)
            .Must(x => homeOrAwayOptions.Contains(x!))
            .When(x => !string.IsNullOrEmpty(x.HomeOrAway))
            .WithMessage("HomeOrAway must be one of: " + string.Join(", ", homeOrAwayOptions));
    }
}