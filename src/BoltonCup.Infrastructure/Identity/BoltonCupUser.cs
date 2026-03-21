using BoltonCup.Core;
using Microsoft.AspNetCore.Identity;

namespace BoltonCup.Infrastructure.Identity;

public class BoltonCupUser : IdentityUser
{
    public int? AccountId { get; set; }
}