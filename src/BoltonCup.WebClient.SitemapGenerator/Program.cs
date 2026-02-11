using System.Diagnostics;
using System.Text;
using CommandLine;
using Serilog;

namespace BoltonCup.WebClient.SitemapGenerator;

internal class Program
{
    static void Main(string[] args)
    {
        Options options = ParseOptions(args);
        ConfigureLogging(options);

        var pages = typeof(Components.Pages.Home).Assembly.GetRoutes();
        
        SitemapBuilder builder = new SitemapBuilder(options.Protocol, options.Domain);
        foreach (var page in pages)
            builder.AddRoute(page);
        var output = builder.Build();
        var filepath = Path.GetFullPath(Path.Combine(options.OutputDirectory, options.OutputFile));
        
        Log.Information("Writing to {FilePath}", filepath);
        Directory.CreateDirectory(options.OutputDirectory);
        using var file = File.Create(filepath);
        using var writer = new StreamWriter(file, Encoding.UTF8);
        writer.Write(output);
    }

    private static void ConfigureLogging(Options options)
    {
        var config = new LoggerConfiguration();
        if (Debugger.IsAttached)
        {
            config.MinimumLevel.Verbose();
        }
        else if (options.DebugLogging)
        {
            config.MinimumLevel.Debug();
        }
        else
        {
            config.MinimumLevel.Information();
        }
        config.Enrich.FromLogContext();
        config.WriteTo.Console();

        Log.Logger = config.CreateLogger();

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Log.Fatal((Exception)e.ExceptionObject, "An unhandled exception has occured");
        };
    }

    private static Options ParseOptions(string[] args)
    {
        using var parser = new Parser(config =>
        {
            config.IgnoreUnknownArguments = true;
            config.EnableDashDash = false;
            config.CaseSensitive = false;
            config.ParsingCulture = System.Globalization.CultureInfo.InvariantCulture;
        });
        var parseResult = parser.ParseArguments<Options>(args);
        return parseResult.Value;
    }
}

