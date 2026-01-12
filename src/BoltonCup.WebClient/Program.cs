using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BoltonCup.WebClient.Components;
using MudBlazor.Services;
using BoltonCup.Sdk;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped(sp =>
{
    var baseUrl = builder.Configuration["BoltonCupApi:BaseUrl"] ?? throw new InvalidOperationException("BoltonCupApi:BaseUrl configuration is missing.");
    var httpClient = sp.GetRequiredService<HttpClient>();
    return new BoltonCupApi(baseUrl, httpClient);
});

builder.Services.AddMudServices();

await builder.Build().RunAsync();