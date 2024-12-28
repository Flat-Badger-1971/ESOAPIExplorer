using System;

namespace ESOAPIExplorer.Models;

public static class Utility
{
    // check if a number is prime
    private static bool IsPrime(int number)
    {
        if (number <= 1)
        {
            return false;
        }

        if (number == 2)
        {
            return true;
        }

        if (number % 2 == 0)
        {
            return false;
        }

        int boundary = (int)Math.Floor(Math.Sqrt(number));

        for (int i = 3; i <= boundary; i += 2)
        {
            if (number % i == 0) return false;
        }

        return true;
    }

    // find the next highest prime number
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
