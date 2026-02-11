using BoltonCup.Common;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BoltonCup.WebClient.Components;
using MudBlazor.Services;
using MudBlazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBoltonCupCommonServices(builder.Configuration);

builder.Services.AddMudServices();
builder.Services.AddMudMarkdownServices();

await builder.Build().RunAsync();