using FluentValidation;

namespace BoltonCup.Core.Queries.Base;

public abstract class SortQueryValidator<T> : AbstractValidator<T>
    where T : ISortQuery
{
    protected SortQueryValidator()
    {
    }
}