using System;
using System.Collections.Generic;

namespace ESOAPIExplorer.Services;

public static class Utility
{
    public static string InferType(string value, IRegexService regexService)
    {
        switch (true)
        {
            case true when IsBooleanExpression(value):
            case true when regexService.BooleanMatcher().IsMatch(value):
            case true when value.EndsWith("enabled", StringComparison.OrdinalIgnoreCase):
                return "boolean";
            case true when regexService.NumberMatcher().IsMatch(value):
            case true when value == "i":
            case true when value.EndsWith("offset", StringComparison.OrdinalIgnoreCase):
            case true when value.EndsWith("padding", StringComparison.OrdinalIgnoreCase):
            case true when value.StartsWith('#'):
            case true when value.EndsWith("index", StringComparison.OrdinalIgnoreCase):
            case true when value.EndsWith("amount", StringComparison.OrdinalIgnoreCase):
                return "number";
            case true when value.StartsWith('"') && value.EndsWith('"'):
            case true when value.EndsWith("Name") || value == "name":
                return "string";
            case true when IsControl(value):
            case true when value == "control":
            case true when value == "data":
            case true when IsObject(value):
                return "userdata";
            case true when value.StartsWith('{') && value.EndsWith('}'):
                return "table";
            case true when value.EndsWith("Function"):
            case true when value == "fn":
                return "function";
            default:
                return "any";
        }
    }

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
            case true when value == "self":
                return true;
            default:
                return false;
        }
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
