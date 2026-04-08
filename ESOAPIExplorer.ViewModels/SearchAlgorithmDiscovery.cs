using ESOAPIExplorer.Models.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ESOAPIExplorer.ViewModels;

public static class SearchAlgorithmDiscovery
{
    public static List<Type> ListSearchAlgorithms()
    {
        Assembly assembly = Assembly.Load("ESOAPIExplorer.Models");

        return assembly
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                t.Namespace == "ESOAPIExplorer.Models.Search" &&
                typeof(ISearchAlgorithm).IsAssignableFrom(t))
            .ToList();
    }

    public static string GetStaticPropertyValue(this Type type, string propertyName)
    {
        PropertyInfo property = type.GetProperty(propertyName);

        if (property == null)
        {
            return null;
        }

        return property.GetValue(null)?.ToString();
    }
}
