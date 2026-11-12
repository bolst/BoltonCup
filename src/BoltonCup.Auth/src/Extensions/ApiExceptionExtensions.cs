using BoltonCup.Sdk;
using BoltonCup.Shared;

namespace BoltonCup.Auth.Extensions;

public static class ApiExceptionExtensions
{
    const string DefaultTooManyRequestsMessage = "Too many attempts. Try again later.";

    /// <summary>
    /// Returns <c>true</c> when the API rejected the request for rate limiting, too many code attempts or lockout. The
    /// API sends all three as 429.
    /// </summary>
    public static bool IsTooManyRequests(this ApiException exception) => exception.StatusCode == 429;

    public static string GetTooManyRequestsMessage(this ApiException exception) =>
        exception is ApiException<BoltonCupProblemDetails> { Result.Detail: { Length: > 0 } detail }
            ? detail
            : DefaultTooManyRequestsMessage;
}