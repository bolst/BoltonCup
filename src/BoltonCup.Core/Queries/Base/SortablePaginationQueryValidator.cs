using BoltonCup.Core.Queries.Base;
using FluentValidation;

namespace BoltonCup.Core.Queries.Base;

public abstract class SortablePaginationQueryValidator<T> : AbstractValidator<T>
    where T : ISortablePaginationQuery
{
    protected SortablePaginationQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be >= 1");        
        
        RuleFor(x => x.Size)
            .InclusiveBetween(1, 100)
            .WithMessage("Size must be an integer in [1,...,100]");
    }
}