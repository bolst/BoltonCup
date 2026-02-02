using Microsoft.AspNetCore.Components;
using Serilog;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using BoltonCup.WebClient.Attributes;

namespace BoltonCup.WebClient.SitemapGenerator;

internal class SitemapBuilder
{
    private const float _defaultPriority = 0.5f;
    private static readonly Regex _invalidRouteRegex = new Regex(@"^[^A-Za-z0-9-&'""<> _/.%\\/]$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private readonly StringBuilder _builder;
    private readonly string _scheme;
    private readonly string _host;

    private string? _result;

    public SitemapBuilder(string scheme, string host)
    {
        _scheme = scheme;
        _host = host;
        _builder = new StringBuilder("<?xml version='1.0' encoding='UTF-8' ?><urlset xmlns = 'http://www.sitemaps.org/schemas/sitemap/0.9'>");
    }

    public void AddRoute(Type pageType)
    {
        Log.Verbose("Getting location for page {PageType}", pageType.FullName);

        // if route has explicit sitemap attribute, this always takes priority
        var sitemapAttribute = pageType.GetCustomAttribute<SitemapAttribute>();

        // if sitemap attribute has null location (or it doesn't exist), we need to parse location from route attribute
        var location = sitemapAttribute?.Location;
        if (string.IsNullOrEmpty(location))
        {
            if (sitemapAttribute is null)
                Log.Debug("Sitemap attribute for {PageType} not found, checking route", pageType.FullName);

            var routeAttribute = pageType.GetCustomAttribute<RouteAttribute>();
            if (routeAttribute is null)
            {
                Log.Error("Type {PageType} does not have a route and isn't a valid Blazor page", pageType.FullName);
                return;
            }
            location = routeAttribute.Template;
        }

        // did not find a location for the page: cannot generate a valid sitemap node for it
        if (string.IsNullOrEmpty(location))
            return;

        DateTimeOffset? modifiedTime = null;
        if (sitemapAttribute?.LastModified != null)
            modifiedTime = DateTimeOffset.Parse(sitemapAttribute.LastModified);

        AddRoute(location, sitemapAttribute?.Priority ?? _defaultPriority, sitemapAttribute?.ChangeFrequency, modifiedTime);
    }

    public void AddRoute(string location, float priority = _defaultPriority, SitemapChangeFrequency? changeFrequency = null, DateTimeOffset? lastModified = null)
    {
        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentNullException(nameof(location));

        Log.Information("Building sitemap node for route {Route}", location);
        if (!location.StartsWith('/') || _invalidRouteRegex.IsMatch(location))
        {
            Log.Error("Route {Route} is not a valid sitemap route", location);
            return;
        }

        var loc = $"{_scheme}://{_host}{Escape(location)}";
        var priorityValue = priority.ToString("0.0", CultureInfo.InvariantCulture);
        var changeFreq = changeFrequency?.ToString().ToLowerInvariant();
        var lastmod = lastModified?.ToString("yyyy-MM-dd");

        lock (_builder)
        {
            _builder.Append("<url>");
            _builder.Append($"<loc>{loc}</loc>");
            _builder.Append($"<priority>{priorityValue}</priority>");
            if (!string.IsNullOrEmpty(changeFreq))
                _builder.Append($"<changefreq>{changeFreq}</changefreq>");
            if (!string.IsNullOrEmpty(lastmod))
                _builder.Append($"<lastmod>{lastmod}</lastmod>");
            _builder.Append("</url>");
        }

        Log.Debug("Sitemap node for route {Route} built: loc = {Location}; priority = {Priority}; changefreq = {ChangeFrequency}; lastmod = {LastModified}",
            location, loc, priorityValue, changeFreq ?? "null", lastmod ?? "null");
    }

    private static string Escape(string input)
    {
        return input
            .Replace("&", "&amp;")
            .Replace("'", "&apos;")
            .Replace("\"", "&quot;")
            .Replace(">", "&gt;")
            .Replace("<", "&lt;")
            .Replace(" ", "%20");
    }

    public string Build()
    {
        if (!string.IsNullOrEmpty(_result)) 
            return _result;
        
        lock (_builder)
        {
            _builder.Append("</urlset>");
            _result = _builder.ToString();
        }

        return _result;
    }

    public override string ToString()
        => Build();
}
