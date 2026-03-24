namespace BoltonCup.Core;

public interface IAccountRepository
{
    Task<IPagedList<Account>> GetAllAsync(GetAccountsQuery query);
    Task<Account?> GetByIdAsync(int id);
}
