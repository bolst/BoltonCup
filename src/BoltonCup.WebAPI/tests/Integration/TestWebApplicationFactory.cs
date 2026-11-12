using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BoltonCup.WebAPI.Tests.Integration;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    const string UnreachableConnectionString = "Host=127.0.0.1;Port=1;Database=none;Username=none;Password=none;Timeout=1";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("BoltonCup:ConnectionString", UnreachableConnectionString);
    }
}