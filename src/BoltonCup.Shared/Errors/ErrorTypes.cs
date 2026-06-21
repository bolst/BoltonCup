namespace BoltonCup.Shared
{
    public static class ErrorTypes
    {
        private const string Base = "https://api.boltoncup.ca/errors";

        public const string NotFound = Base + "/not-found";
        public const string TooManyRequests = Base + "/too-many-requests";
        public const string Unexpected = Base + "/unexpected";
        public const string Validation = Base + "/validation";

        public static class Auth
        {
            private const string Base = ErrorTypes.Base + "/auth";
            
            public const string AccountNotConfirmed = Base + "/account-not-confirmed";
            public const string InvalidCredentials = Base + "/invalid-credentials";
            public const string UserRegistrationFailed = Base + "/user-registration-failed";
        }
        
        
        
        public static class Tournaments
        {
            private const string Base = ErrorTypes.Base + "/tournaments";
            
            public const string AccountAlreadyRegistered = Base + "/account-already-registered";
            public const string AccountNotRegistered = Base + "/account-not-registered";
            public const string RegistrationClosed = Base + "/registration-closed";
        }


        public static class BracketChallenges
        {
            private const string Base = ErrorTypes.Base + "/bracket-challenges";

            public const string EmailAlreadyRegistered = Base + "/email-already-registered";
            public const string RegistrationClosed = Base + "/registration-closed";
        }


        public static class Trades
        {
            private const string Base = ErrorTypes.Base + "/trades";

            public const string TradingClosed = Base + "/trading-closed";
            public const string InvalidState = Base + "/invalid-state";
            public const string PlayerLocked = Base + "/player-locked";
            public const string PlayerNotTradeable = Base + "/player-not-tradeable";
            public const string InvalidRoster = Base + "/invalid-roster";
        }
    }
}
