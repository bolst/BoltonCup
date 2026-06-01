using BoltonCup.Common;
using BoltonCup.Timekeeper.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BoltonCup.Timekeeper.Components;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBoltonCupCommonServices(builder.Configuration);
builder.Logging.AddBoltonCupSentry(builder.Configuration);

if (builder.HostEnvironment.IsProduction())
{
    builder.Logging.SetMinimumLevel(LogLevel.None);
}

builder.Services.AddMudServices();
builder.Services.AddScoped<TimekeeperStateService>();

await builder.Build().RunAsync();
