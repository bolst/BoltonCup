namespace BoltonCup.Shared
{
    public static class BoltonCupClaimTypes
    {
        public const string AccountId = "AccountId";
        public const string TournamentGm = "TournamentGm";
        public const string TeamGm = "TeamGm";

        // Set while an admin is masquerading as another user; records the original admin to return to.
        public const string OriginalUserId = "OriginalUserId";
        public const string OriginalUserName = "OriginalUserName";
    }
}