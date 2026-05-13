using System.Security.Claims;
using BoltonCup.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Tests.Helpers;

internal static class ClaimsHelper
{
    public static ControllerContext WithAccountId(int accountId)
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(BoltonCupClaimTypes.AccountId, accountId.ToString()) },
            "TestAuth");

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }
}
