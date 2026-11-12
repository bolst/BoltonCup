using Microsoft.AspNetCore.Identity;

namespace BoltonCup.Persistence.Identity;

/// <remarks>The account a user owns is linked from the account side, by <c>Account.UserId</c>.</remarks>
public class BoltonCupUser : IdentityUser;