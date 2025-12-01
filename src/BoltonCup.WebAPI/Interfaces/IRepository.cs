

namespace BoltonCup.WebAPI.Interfaces;

public interface IRepository<TModel, in TIndex> where TModel : class
{
    Task<IEnumerable<TModel>> GetAllAsync();
    Task<TModel?> GetByIdAsync(TIndex id);
    Task<bool> AddAsync(TModel entity);
    Task<bool> UpdateAsync(TModel entity);
    Task<bool> DeleteAsync(TIndex id);
}
