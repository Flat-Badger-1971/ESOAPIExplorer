using System;
using System.Collections.Generic;

namespace ESOAPIExplorer.Models;

public static class Utility
{
    public static bool IsControl(string value)
    {
        switch (value)
        {
            case "control":
            case "button":
            case "label":
            case "checkbox":
                return true;
            default:
                return false;
        }
    }

    public static bool IsObject(string value)
    {
        switch (true)
        {
            case true when value.StartsWith("ZO_", StringComparison.OrdinalIgnoreCase):
            case true when value.EndsWith("node", StringComparison.OrdinalIgnoreCase):
                return true;
            default:
                return false;
        }
    }

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
    private static HashSet<string> variables = new HashSet<string>();

    public static bool IsBooleanExpression(string expression)
    {
        // Remove whitespace for easier processing
        expression = expression.Replace(" ", string.Empty);

        // Check if the expression is valid
        return IsValidExpression(expression);
    }

    private static bool IsValidExpression(string expression)
    {
        if (string.IsNullOrEmpty(expression))
            return false;

        // Check for balanced parentheses
        if (!AreParenthesesBalanced(expression))
            return false;

        // Check for constants true and false
        if (expression == "true" || expression == "false")
            return true;

        // Check for variables
        if (variables.Contains(expression))
            return true;

        // Check for negation
        if (expression.StartsWith("not"))
            return IsValidExpression(expression.Substring(3));

        // Check for comparison operators
        if (ContainsComparisonOperator(expression))
            return true;

        // Check for conjunction and disjunction
        int index = FindMainOperator(expression);
        if (index != -1)
        {
            string left = expression.Substring(0, index);
            string right = expression.Substring(index + 2);

            return IsValidExpression(left) && IsValidExpression(right);
        }

        return false;
    }

    private static bool AreParenthesesBalanced(string expression)
    {
        Stack<char> stack = new Stack<char>();
        foreach (char ch in expression)
        {
            if (ch == '(')
                stack.Push(ch);
            else if (ch == ')')
            {
                if (stack.Count == 0)
                    return false;
                stack.Pop();
            }
        }
        return stack.Count == 0;
    }

    private static bool ContainsComparisonOperator(string expression)
    {
        string[] comparisonOperators = { "==", "~=", "<", "<=", ">", ">=" };
        foreach (var op in comparisonOperators)
        {
            if (expression.Contains(op))
                return true;
        }
        return false;
    }

    private static int FindMainOperator(string expression)
    {
        Stack<char> stack = new Stack<char>();
        for (int i = 0; i < expression.Length; i++)
        {
            if (expression[i] == '(')
                stack.Push('(');
            else if (expression[i] == ')')
                stack.Pop();
            else if (stack.Count == 0)
            {
                if (expression.Substring(i).StartsWith("and") || expression.Substring(i).StartsWith("or"))
                {
                    return i;
                }
            }
        }
        return -1;
    }

    public static void AddVariable(string variable)
    {
        variables.Add(variable);
    }
}
