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
            public const string RegistrationClosed = Base + "/registration-closed";
        }
    }
}
