using Microsoft.AspNetCore.Components;
using System.Reflection;
using BoltonCup.Shared.Services;

namespace BoltonCup.Web.Services;

public class SitemapService : ISitemapService
{
    public IEnumerable<Type> GetPages()
    {
        var pages = Assembly.GetExecutingAssembly().ExportedTypes.Where(p =>
            p.IsSubclassOf(typeof(ComponentBase)) && p.Namespace == "BoltonCup.Web.Components.Pages");
        return pages;
    }
}
