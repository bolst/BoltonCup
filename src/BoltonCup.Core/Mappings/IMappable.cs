using System.Linq.Expressions;

namespace BoltonCup.Core.Mappings;

public interface IMappable<TSource, TResult>
{
    static abstract Expression<Func<TSource, TResult>> Projection { get; }
}