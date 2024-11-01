using MudBlazor.Services;
using BoltonCup.Components;
using BoltonCup.Data;
using Blazored.LocalStorage;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddMemoryCache();

builder.Services.AddSingleton<ICacheService, CacheService>();

builder.Services.AddScoped<IBCData>(sp =>
{
    var connectionString = Environment.GetEnvironmentVariable("SB_CSTRING");
    var cacheService = sp.GetRequiredService<ICacheService>();
    return new BCData(connectionString!, cacheService);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
