using System;

namespace PG.StarWarsGame.Engine.Utilities;

internal static class StringUtilities
{
    internal static unsafe string Concat(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1, ReadOnlySpan<char> str2)
    {
        var result = new string('\0', checked(str0.Length + str1.Length + str2.Length));
        fixed (char* resultPtr = result)
        {
            var resultSpan = new Span<char>(resultPtr, result.Length);

            str0.CopyTo(resultSpan);
            resultSpan = resultSpan.Slice(str0.Length);

            str1.CopyTo(resultSpan);
            resultSpan = resultSpan.Slice(str1.Length);

            str2.CopyTo(resultSpan);
        }
        return result;
    }
}