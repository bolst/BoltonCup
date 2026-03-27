namespace BoltonCup.Infrastructure.Identity;

public static class BoltonCupRole
{
    public const string User = "User";
    public const string Admin = "Admin";

    public static IReadOnlyList<string> All = [User, Admin];
}