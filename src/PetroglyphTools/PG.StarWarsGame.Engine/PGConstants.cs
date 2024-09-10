using System.Text;

namespace PG.StarWarsGame.Engine;

public static class PGConstants
{
    public static readonly Encoding PGCrc32Encoding = Encoding.ASCII;

    // Reserved one character for the null-terminator
    public const int MaxPathLength = 259;
}