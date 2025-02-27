using MudBlazor.Services;
using BoltonCup.Web.Components;
using BoltonCup.Shared.Data;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(BoltonCup.Shared.Components.Pages.Home).Assembly);

app.Run();
