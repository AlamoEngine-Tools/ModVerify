using System;
using System.Collections.Generic;

namespace PG.StarWarsGame.Engine.Utilities;

internal static class ExtensionMethods
{
    extension<T>(List<T> list)
    {
        public void ClearAddRange(IEnumerable<T> items)
        {
            list.Clear();
            list.AddRange(items);   
        }
    }
    
    public static int Atoi(this ReadOnlySpan<char> value)
    {
        var i = 0;

        // Skip leading whitespace
        while (i < value.Length && char.IsWhiteSpace(value[i]))
            i++;

        if (i >= value.Length)
            return 0;

        // Handle optional sign
        var sign = 1;
        switch (value[i])
        {
            case '-':
                sign = -1;
                i++;
                break;
            case '+':
                i++;
                break;
        }

        // Parse digits
        long result = 0;  // Use long to detect overflow
        while (i < value.Length && char.IsDigit(value[i]))
        {
            result = result * 10 + (value[i] - '0');

            switch (sign)
            {
                case 1 when result > int.MaxValue:
                    return int.MaxValue;
                case -1 when -result < int.MinValue:
                    return int.MinValue;
                default:
                    i++;
                    break;
            }
        }

        return (int)(sign * result);
    }
}