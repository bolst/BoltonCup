using BoltonCup.Common;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BoltonCup.WebClient.Components;
using BoltonCup.WebClient.Services;
using MudBlazor.Services;
using MudBlazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<ITournamentRegistrationService, TournamentRegistrationService>();

builder.Services.AddBoltonCupCommonServices(builder.Configuration);

builder.Logging.AddSentry(options =>
{
    options.Dsn = builder.Configuration["Sentry::Dsn"] ?? string.Empty;
    options.TracesSampleRate = 1.0;
});

builder.Services.AddMudServices();
builder.Services.AddMudMarkdownServices();

await builder.Build().RunAsync();