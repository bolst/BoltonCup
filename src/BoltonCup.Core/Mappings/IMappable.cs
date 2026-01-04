using System.Linq.Expressions;

namespace BoltonCup.Core.Mappings;

public interface IMappable<TSource, TResult>
{
    Expression<Func<TSource, TResult>> Projection { get; }
}