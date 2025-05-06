using System.Diagnostics;
using System.Reflection;
using Blazored.LocalStorage;
using BoltonCup.Shared.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Supabase;

namespace BoltonCup.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		using var stream = Assembly
			.GetExecutingAssembly()
			.GetManifestResourceStream("BoltonCup.App.appsettings.txt");
		if (stream == null)
		{
			Console.WriteLine("Failed to load appsettings");
		}
		else
		{
			var config = new ConfigurationBuilder()
				.AddJsonStream(stream)
				.Build();
			builder.Configuration.AddConfiguration(config);
		}
		
		builder.Services.AddMauiBlazorWebView();

		builder.Services.AddBlazoredLocalStorage();
		builder.Services.AddScoped<ICustomLocalStorageProvider, Data.CustomLocalStorageProvider>();

		builder.Services.AddBoltonCupServices(builder.Configuration);
		builder.Services.AddBoltonCupSupabase(builder.Configuration);
		builder.Services.AddBoltonCupAuth();

		builder.Services.AddMudServices();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
