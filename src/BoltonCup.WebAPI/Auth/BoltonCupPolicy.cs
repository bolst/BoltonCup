namespace BoltonCup.WebAPI.Auth;

/// <summary>Defines authorization policy names used across the BoltonCup API.</summary>
public static partial class BoltonCupPolicy
{
    /// <summary>Policy that requires the user to have a completed account with an account ID claim.</summary>
    public const string RequireCompletedAccount = "RequireCompletedAccount";
}