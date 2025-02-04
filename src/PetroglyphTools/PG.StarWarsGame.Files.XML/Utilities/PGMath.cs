using System;
using System.Runtime.CompilerServices;

namespace PG.StarWarsGame.Files.XML.Utilities;

internal static class PGMath
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Clamp(int value, int min, int max)
    {
        if (min > max)
            throw new ArgumentException("min cannot be larger than max.");
        if (value < min)
            return min;
        return value > max ? max : value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte Clamp(byte value, byte min, byte max)
    {
        if (min > max)
            throw new ArgumentException("min cannot be larger than max.");
        if (value < min)
            return min;
        return value > max ? max : value;
    }
}