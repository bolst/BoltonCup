using BoltonCup.Core.Interfaces.Base;

namespace BoltonCup.Core;

public interface IAccountRepository : IRepository<Account, GetAccountsQuery, int>
{
}
