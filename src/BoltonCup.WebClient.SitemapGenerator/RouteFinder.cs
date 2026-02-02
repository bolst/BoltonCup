using Microsoft.AspNetCore.Components;
using System.Reflection;
using BoltonCup.WebClient.Attributes;

namespace BoltonCup.WebClient.SitemapGenerator;

internal static class RouteFinder
{
    public static IEnumerable<Type> GetRoutes(this Assembly assembly) => 
        assembly.ExportedTypes.Where(type => 
            type is { IsClass: true, IsAbstract: false } 
             // pages are always non-abstract classes that inherit from ComponentBase
             && type.IsSubclassOf(typeof(ComponentBase)) 
             // they also always have route attribute
             && type.GetCustomAttribute<RouteAttribute>() != null
             // additionally, we have a special attribute to explicitly ignore the page
             && type.GetCustomAttribute<SitemapIgnoreAttribute>() == null);
}