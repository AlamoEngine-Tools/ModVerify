using System;

namespace PG.StarWarsGame.Engine.FileSystem.Test;

internal static class PathAssert
{
    public static void Equal(ReadOnlySpan<char> expected, ReadOnlySpan<char> actual)
    {
        if (!actual.SequenceEqual(expected))
            throw Xunit.Sdk.EqualException.ForMismatchedValues(expected.ToString(), actual.ToString());
    }

    public static void Empty(ReadOnlySpan<char> actual)
    {
        if (actual.Length > 0)
            throw Xunit.Sdk.NotEmptyException.ForNonEmptyCollection();
    }
}