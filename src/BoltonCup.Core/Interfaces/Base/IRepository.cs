

using BoltonCup.Core.Mappings;

namespace BoltonCup.Core.Base;

public interface IRepository<TModel, in TGetQuery, in TKey> where TModel : class
{
    Task<IEnumerable<TModel>> GetAllAsync(TGetQuery query);
    Task<IEnumerable<TResult>> GetAllAsync<TResult>(TGetQuery query)
        where TResult : IMappable<TModel, TResult>;
    
    Task<TModel?> GetByIdAsync(TKey id);
    Task<TResult?> GetByIdAsync<TResult>(TKey id)
        where TResult : IMappable<TModel, TResult>;
    
    Task<bool> AddAsync(TModel entity);
    Task<bool> UpdateAsync(TModel entity);
    Task<bool> DeleteAsync(TKey id);
}