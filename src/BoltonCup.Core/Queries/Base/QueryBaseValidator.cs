using FluentValidation;

namespace BoltonCup.Core.Queries.Base;

public abstract class QueryBaseValidator<T> : AbstractValidator<T>
    where T : IQueryBase
{
    protected QueryBaseValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be >= 1");        
        
        RuleFor(x => x.Size)
            .InclusiveBetween(1, 100)
            .WithMessage("Size must be an integer in [1,...,100]");
    }
}