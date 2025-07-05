using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace BoltonCup.Shared.Data;

public class SitemapService : ISitemapService
{
    public IEnumerable<Type> GetPages()
    {
        var pages = Assembly.GetExecutingAssembly().ExportedTypes.Where(p =>
            p.IsSubclassOf(typeof(ComponentBase)) && p.Namespace == "BoltonCup.Shared.Components.Pages");
        return pages;
    }
}
