using BoltonCup.Core.Queries.Base;
using BoltonCup.Core.Values;
using FluentValidation;

namespace BoltonCup.Core.Queries;

public class GetSkaterStatsQueryValidator : PaginationQueryValidator<GetSkaterStatsQuery>
{
    public GetSkaterStatsQueryValidator()
    {
        var positions = new List<string>() { Position.Forward, Position.Defense };
        RuleFor(x => x.Position)
            .Must(x => positions.Contains(x!))
            .When(x => !string.IsNullOrEmpty(x.Position))
            .WithMessage("Position must be one of: " + string.Join(", ", positions));
    }
}