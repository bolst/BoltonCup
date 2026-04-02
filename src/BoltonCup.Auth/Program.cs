using BoltonCup.Common;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BoltonCup.Auth.Components;
using BoltonCup.Auth.Services;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBoltonCupCommonServices(builder.Configuration);

builder.Logging.AddSentry(options =>
{
    options.Dsn = builder.Configuration["Sentry::Dsn"] ?? string.Empty;
    options.TracesSampleRate = 1.0;
});

builder.Services.AddScoped<AuthSessionStateService>();

builder.Services.AddMudServices();

await builder.Build().RunAsync();