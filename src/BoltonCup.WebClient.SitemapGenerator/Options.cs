using CommandLine;

namespace BoltonCup.WebClient.SitemapGenerator;

internal sealed class Options
{
    [Option('o', "output", Default = "bin/publish/wwwroot")]
    public string OutputDirectory { get; }
    
    [Option('f', "filename", Default = "sitemap.xml")]
    public string OutputFile { get; }
    
    [Option('d', "domain", Default = "boltoncup.ca")]
    public string Domain { get; }
    
    [Option('p', "protocol", Default = "https")]
    public string Protocol { get; }
    
    [Option("debug", Default = false)]
    public bool DebugLogging { get; }
    
    

    public Options(string outputDirectory, string outputFile, string domain, string protocol, bool debugLogging)
    {
        OutputDirectory = outputDirectory;
        OutputFile = outputFile;
        Domain = domain;
        Protocol = protocol;
        DebugLogging = debugLogging;
    }
}
