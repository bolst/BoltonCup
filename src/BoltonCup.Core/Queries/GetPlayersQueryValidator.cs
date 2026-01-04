using FluentValidation;

namespace BoltonCup.Core.Queries;

public class GetPlayersQueryValidator : AbstractValidator<GetPlayersQuery>
{
    public GetPlayersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("ur gay");
    }
}