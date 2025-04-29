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
		
		builder.Services.AddScoped(provider =>
		{
			var url = builder.Configuration["SUPABASE_URL"];
			var key = builder.Configuration["SUPABASE_KEY"];

			if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(key))
			{
				throw new InvalidOperationException("SET YOUR ENV VARIABLES!\n");
			}
            
			return new Supabase.Client(url, key, new SupabaseOptions
			{
				AutoConnectRealtime = true,
			});
		});
		
		// Authorization
		builder.Services.AddAuthorizationCore();
		builder.Services.AddScoped<CustomUserService>();
		builder.Services.AddScoped<CustomAuthenticationStateProvider>();
		builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthenticationStateProvider>());

		builder.Services.AddMudServices();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
