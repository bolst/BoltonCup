using BoltonCup.Common;
using BoltonCup.Timekeeper.Services;
using BoltonCup.Timekeeper.Services.Music;
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
builder.Services.AddSingleton<IOfflineStore, LocalStorageOfflineStore>();
builder.Services.AddSingleton<SyncService>();
builder.Services.AddScoped<TimekeeperStateService>();
builder.Services.AddScoped<MusicCacheService>();
builder.Services.AddScoped<MusicPlayerService>();

var host = builder.Build();
_ = host.Services.GetRequiredService<SyncService>().StartAsync();
await host.RunAsync();
