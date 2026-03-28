namespace BoltonCup.Shared;

public static class ErrorTypes
{
    private const string Base = "https://api.boltoncup.ca/errors";

    public const string NotFound = $"{Base}/not-found";
    public const string Validation = $"{Base}/validation";
    public const string Unexpected = $"{Base}/unexpected";

    public static class Tournaments
    {
        private const string Base = $"{ErrorTypes.Base}/tournaments";
        
        public const string AccountAlreadyRegistered = $"{Base}/account-already-registered";
        public const string RegistrationClosed = $"{Base}/registration-closed";
    }
}