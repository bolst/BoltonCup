using Blazored.LocalStorage;
using BoltonCup.Draft.Components;
using BoltonCup.Draft.Data;
using BoltonCup.Shared.Data;
using BoltonCup.Draft.Hubs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddBlazoredLocalStorage();
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

builder.Services.AddScoped<DraftServiceProvider>();

builder.Services.AddSignalR();

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

app.MapHub<DraftHub>(DraftHub.HubUrl);

app.Run();