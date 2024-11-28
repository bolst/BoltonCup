using MudBlazor.Services;
using BoltonCup.Components;
using BoltonCup.Data;
using Blazored.LocalStorage;
using Supabase;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

// Add services to the container.
builder.Services.AddMemoryCache();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<Supabase.Client>(sp =>
{
    var url = Environment.GetEnvironmentVariable("SUPABASE_URL");
    var key = Environment.GetEnvironmentVariable("SUPABASE_KEY");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var options = new SupabaseOptions
    {
        AutoRefreshToken = true,
        AutoConnectRealtime = true,
        SessionHandler = new CustomSupabaseSessionHandler(localStorage),
    };
    return new Supabase.Client(url, key, options);
});

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddScoped<CustomUserService>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthenticationStateProvider>());

builder.Services.AddScoped<OTPLogin>();
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
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
