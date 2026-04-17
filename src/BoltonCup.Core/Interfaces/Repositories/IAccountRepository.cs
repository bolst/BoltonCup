namespace BoltonCup.Core;

public interface IAccountRepository
{
    Task<IPagedList<Account>> GetAllAsync(GetAccountsQuery query, CancellationToken cancellationToken = default);
    Task<Account?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
