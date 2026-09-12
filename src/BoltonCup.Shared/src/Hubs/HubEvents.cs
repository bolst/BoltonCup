namespace BoltonCup.Shared
{
    public static class HubEvents
    {
        public static class Draft
        {
            public const string OnDraftUpdate = "OnDraftUpdate";
            public const string OnDraftStatusChange = "OnDraftStatusChange";
            public const string OnPickMade = "OnPickMade";
            public const string OnAutoPickStateChange = "OnAutoPickStateChange";
        }
    }
}