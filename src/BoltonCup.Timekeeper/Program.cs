using BoltonCup.Common;
using BoltonCup.Timekeeper.Services;
using BoltonCup.Timekeeper.Services.Music;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BoltonCup.Timekeeper.Components;
using MudBlazor;
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

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Text;
    config.SnackbarConfiguration.VisibleStateDuration = 2500;
    config.SnackbarConfiguration.ShowTransitionDuration = 1000;
    config.SnackbarConfiguration.HideTransitionDuration = 1000;
});
builder.Services.AddSingleton<IOfflineStore, LocalStorageOfflineStore>();
builder.Services.AddSingleton<SyncService>();
builder.Services.AddScoped<TimekeeperStateService>();
builder.Services.AddScoped<DeviceStorageService>();
builder.Services.AddScoped<MusicCacheService>();
builder.Services.AddScoped<MusicPlayerService>();
builder.Services.AddScoped<MusicDownloadService>();

var host = builder.Build();
_ = host.Services.GetRequiredService<SyncService>().StartAsync();
await host.RunAsync();
