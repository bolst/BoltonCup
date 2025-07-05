using Blazored.LocalStorage;
using MudBlazor.Services;
using BoltonCup.Web.Components;
using BoltonCup.Web.Data;
using BoltonCup.Shared.Data;


var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add local storage
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<ICustomLocalStorageProvider, CustomLocalStorageProvider>();

// Core services
builder.Services.AddBoltonCupServices(builder.Configuration);
builder.Services.AddBoltonCupSupabase(builder.Configuration);

builder.Services.AddDataProtection();

builder.Services.AddScoped<StripeServiceProvider>(sp =>
{
    var apiKey = builder.Configuration["STRIPE_API_KEY"]; 
    
    if (string.IsNullOrWhiteSpace(apiKey))
    {
        throw new InvalidOperationException("SET YOUR (Stripe) ENV VARIABLES!\n");
    }
    
    var bcData = sp.GetRequiredService<IBCData>();
    return new StripeServiceProvider(apiKey, bcData);
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
    .AddAdditionalAssemblies(typeof(BoltonCup.Shared.Components.Pages.Info).Assembly);

app.UseStatusCodePagesWithRedirects("/404?status={0}");

app.MapGet("/sitemap.xml", async httpContext =>
{
    var webPages = new BoltonCup.Web.Services.SitemapService().GetPages();
    var sharedPages = new BoltonCup.Shared.Data.SitemapService().GetPages();
    var pages = webPages.Concat(sharedPages);
    var sitemap = ISitemapService.GenerateSitemap(pages);
    
    await httpContext.Response.WriteAsync(sitemap);
});

app.Run();
