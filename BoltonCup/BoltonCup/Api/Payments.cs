namespace BoltonCup.Api
{
    public static class Payments
    {
        public readonly static string STRIPE_KEY = Environment.GetEnvironmentVariable("STR_KEY") ?? "";
        public readonly static string PROD_ID = Environment.GetEnvironmentVariable("STR_PRI") ?? "";

    }

}