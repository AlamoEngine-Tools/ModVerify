using System;
using System.Runtime.CompilerServices;
#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace PG.StarWarsGame.Files.XML.Utilities;

internal static class PGMath
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Clamp(int value, int min, int max)
    {
#if NETSTANDARD2_1_OR_GREATER || NET
        return Math.Clamp(value, min, max);
#endif
        if (min > max)
            throw new ArgumentException($"'{max}' cannot be greater than {min}.");
        if (value < min)
            return min;
        return value > max ? max : value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte Clamp(byte value, byte min, byte max)
    {
#if NETSTANDARD2_1_OR_GREATER || NET
        return Math.Clamp(value, min, max);
#endif
        if (min > max)
            throw new ArgumentException($"'{max}' cannot be greater than {min}.");
        if (value < min)
            return min;
        return value > max ? max : value;
    }

#if NET7_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Clamp<T>(T value, T min, T max) where T : INumber<T>
    {
        if (min > max)
            throw new ArgumentException($"'{max}' cannot be greater than {min}.");
        if (value < min)
            return min;
        return value > max ? max : value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Max<T>(T val1, T val2) where T : INumber<T>
    {
        return (val1 >= val2) ? val1 : val2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Min<T>(T val1, T val2) where T : INumber<T>
    {
        return (val1 <= val2) ? val1 : val2;
    }
# else

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Clamp<T>(T value, T min, T max) where T : struct, IEquatable<T>, IComparable<T>
    {
        if (min.CompareTo(max) > 0)
            throw new ArgumentException($"'{max}' cannot be greater than {min}.");
        if (value.CompareTo(min) < 0)
            return min;
        return value.CompareTo(max) > 0 ? max : value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Max<T>(T val1, T val2) where T : struct, IEquatable<T>, IComparable<T>
    {
        return val1.CompareTo(val2) >= 0 ? val1 : val2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Min<T>(T val1, T val2) where T : struct, IEquatable<T>, IComparable<T>
    {
        return val1.CompareTo(val2) <= 0 ? val1 : val2;
    }
#endif
}