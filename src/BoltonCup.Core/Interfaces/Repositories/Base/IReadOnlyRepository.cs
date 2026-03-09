using BoltonCup.Core.Mappings;

namespace BoltonCup.Core.Interfaces.Base;

public interface IReadOnlyRepository<TModel, in TGetQuery, in TKey> where TModel : class
{
    Task<CollectionResult<TModel>> GetAllAsync(TGetQuery query);
    Task<CollectionResult<TResult>> GetAllAsync<TResult>(TGetQuery query)
        where TResult : IMappable<TModel, TResult>;
    
    Task<TModel?> GetByIdAsync(TKey id);
    Task<TResult?> GetByIdAsync<TResult>(TKey id)
        where TResult : IMappable<TModel, TResult>;
}