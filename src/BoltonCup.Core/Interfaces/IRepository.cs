

namespace BoltonCup.Core;

public interface IRepository<TModel, in TKey> where TModel : class
{
    Task<IEnumerable<TModel>> GetAllAsync();
    Task<TModel?> GetByIdAsync(TKey id);
    Task<bool> AddAsync(TModel entity);
    Task<bool> UpdateAsync(TModel entity);
    Task<bool> DeleteAsync(TKey id);
}
