namespace BoltonCup.Core;

public interface IAccountRepository
{
    Task<IPagedList<Account>> GetAllAsync(GetAccountsQuery query);
    Task<Account?> GetByIdAsync(int id);
    Task<bool> AddAsync(Account entity);
    Task<bool> UpdateAsync(Account entity);
    Task<bool> DeleteAsync(int id);
}
