using System.Text;

namespace BoltonCup.Shared.Data;

public interface ISitemapService
{
    public IEnumerable<Type> GetPages();
    
    
    public string Sitemap => GenerateSitemap(GetPages());
    
    public static string GenerateSitemap(IEnumerable<Type> pages)
    {
        var sb = new StringBuilder();
        sb.AppendLine("""<?xml version="1.0" encoding="utf-8"?>""");
        sb.AppendLine("""<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">""");
        foreach (var page in pages)
        {
            string pageName = page.Name.ToLower();
            if (pageName == "home") pageName = "";
            sb.AppendLine("<url>");
            sb.AppendLine($"<loc>https://boltoncup.ca/{pageName}</loc>");
            sb.AppendLine("</url>");
        }
        sb.AppendLine("</urlset>");
        
        return sb.ToString();
    }
}