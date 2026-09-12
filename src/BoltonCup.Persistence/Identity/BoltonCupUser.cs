using BoltonCup.Core;
using Microsoft.AspNetCore.Identity;

namespace BoltonCup.Persistence.Identity;

public class BoltonCupUser : IdentityUser
{
    public int? AccountId { get; set; }
}