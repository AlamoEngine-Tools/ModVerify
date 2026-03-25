using System;

namespace PG.StarWarsGame.Engine.Utilities;

internal static class PGMath
{
#if NETSTANDARD2_1_OR_GREATER || NET
    public static float Floor(float value) => MathF.Floor(value);
#else
    public static float Floor(float value) => (float)Math.Floor(value);
#endif
}