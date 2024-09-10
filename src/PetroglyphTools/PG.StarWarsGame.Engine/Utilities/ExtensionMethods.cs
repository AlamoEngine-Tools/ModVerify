using System;
using System.Collections.Generic;

namespace PG.StarWarsGame.Engine.Utilities;

internal static class ExtensionMethods
{
    public static int CopyTo<T>(this ICollection<T> entries, Span<T> entryBuffer)
    {
        if (entryBuffer.Length < entries.Count)
            throw new ArgumentException("buffer is too small", nameof(entryBuffer));

        var count = 0;
        foreach (var entry in entries)
            entryBuffer[count++] = entry;

        return count;
    }
}