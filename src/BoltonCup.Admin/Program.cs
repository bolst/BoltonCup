using BoltonCup.Admin.Components;
using BoltonCup.Common;
using BoltonCup.Infrastructure;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Sdk;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

var keyDirectory = builder.Configuration["DataProtection:KeyDirectory"];
if (!string.IsNullOrEmpty(keyDirectory))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keyDirectory))
        .SetApplicationName("BoltonCup.SharedAuth");
}

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBoltonCupCommonServices(builder.Configuration);

var connectionString = builder.Configuration.GetValue<string>(ConfigurationPaths.ConnectionString);
builder.Services
    .AddDbContext<BoltonCupDbContext>(options => options.UseNpgsql(connectionString))
    .AddDbContext<AuthDbContext>(options => options.UseNpgsql(connectionString));


var configSection = builder.Configuration.GetSection(BoltonCupConfiguration.SectionName);
var bcConfig = configSection.Get<BoltonCupConfiguration>() 
               ?? throw new ArgumentException("Missing Bolton Cup configuration.", nameof(BoltonCupConfiguration));

builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
builder.Services.AddHttpClient("BoltonCupApi")
    .ConfigureHttpClient(client => 
    {
        client.BaseAddress = new Uri(bcConfig.ApiBaseUrl);
    })
    .AddTypedClient((http, _) => new BoltonCupApi(bcConfig.ApiBaseUrl, http));

builder.Services.AddAuthentication("Identity.Application")
    .AddCookie("Identity.Application", options =>
    {
        options.Cookie.Name = ".BoltonCup.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        if (builder.Environment.IsProduction())
        {
            options.Cookie.Domain = ".boltoncup.ca";
        }

        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;

        options.Events.OnRedirectToLogin = context =>
        {
            var returnUrl = Uri.EscapeDataString(context.Request.GetEncodedUrl());
            
            context.Response.Redirect($"{bcConfig.AuthBaseUrl}?returnUrl={returnUrl}");
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddMudServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();