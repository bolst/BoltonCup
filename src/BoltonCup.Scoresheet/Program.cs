using BoltonCup.Scoresheet.Components;
using BoltonCup.Shared.Data;
using MudBlazor.Services;
using Blazored.LocalStorage;
using BoltonCup.Scoresheet.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddDataProtection();
builder.Services.AddScoped<ICustomLocalStorageProvider, CustomLocalStorageProvider>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
        options =>
        {
            options.LoginPath = new PathString("/login");
            options.AccessDeniedPath = new PathString("/auth/denied");
        });

builder.Services.AddBoltonCupServices(builder.Configuration);
builder.Services.AddBoltonCupSupabase(builder.Configuration);
builder.Services.AddBoltonCupAuth();

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
    .AddAdditionalAssemblies(typeof(BoltonCup.Shared.Components.Pages.Info).Assembly);

app.Run();