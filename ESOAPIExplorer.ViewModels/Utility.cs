using ESOAPIExplorer.Models.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ESOAPIExplorer.ViewModels;

public static class Utility
{
    public static List<Type> ListSearchAlgorithms()
    {
        return
            Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                t.Namespace == "EsoAPIExplorer.Models.Search" &&
                typeof(ISearchAlgorithm).IsAssignableFrom(t))
            .ToList();
    }
}
