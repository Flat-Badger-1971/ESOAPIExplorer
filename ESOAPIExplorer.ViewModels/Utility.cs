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
        Assembly assembly = Assembly.Load("ESOAPIExplorer.Models");

        return
            assembly
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                t.Namespace == "ESOAPIExplorer.Models.Search" &&
                typeof(ISearchAlgorithm).IsAssignableFrom(t))
            .ToList();
    }

    // Type extension method
    public static string GetPropertyValue(this Type type, string propertyName)
    {
        PropertyInfo property = type.GetProperty(propertyName);

        if (property == null)
        {
            return null;
        }

        return property.GetValue(null).ToString();
    }

    // Method to check if a number is prime
    private static bool IsPrime(int number)
    {
        if (number <= 1) return false;
        if (number == 2) return true;
        if (number % 2 == 0) return false;

        var boundary = (int)Math.Floor(Math.Sqrt(number));

        for (int i = 3; i <= boundary; i += 2)
        {
            if (number % i == 0) return false;
        }

        return true;
    }

    // Method to find the next highest prime number
    public static int NextPrime(int number)
    {
        int nextNumber = number + 1;

        while (!IsPrime(nextNumber))
        {
            nextNumber++;
        }

        return nextNumber;
    }
}
