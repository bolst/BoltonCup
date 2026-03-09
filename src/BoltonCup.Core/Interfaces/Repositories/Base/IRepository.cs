namespace BoltonCup.Core.Interfaces.Base;

public interface IRepository<TModel, in TGetQuery, in TKey> 
    : IReadOnlyRepository<TModel, TGetQuery, TKey> where TModel : class
{
    Task<bool> AddAsync(TModel entity);
    Task<bool> UpdateAsync(TModel entity);
    Task<bool> DeleteAsync(TKey id);
}