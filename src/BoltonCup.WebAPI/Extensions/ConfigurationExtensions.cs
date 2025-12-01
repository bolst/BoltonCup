

namespace BoltonCup.WebAPI;

public static class ConfigurationExtensions
{
    public static string GetBoltonCupConnectionString(this IConfiguration configuration) 
        => configuration.GetConnectionString("DefaultConnection") 
           ?? throw new Exception("Connection string is null");
}