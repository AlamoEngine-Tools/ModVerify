using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace PG.StarWarsGame.Engine.IO;

public interface IRepository
{
    Stream OpenFile(string filePath, bool megFileOnly = false);
    Stream OpenFile(ReadOnlySpan<char> filePath, bool megFileOnly = false);

    bool FileExists(string filePath, bool megFileOnly = false);
    bool FileExists(string filePath, bool megFileOnly, out bool inMeg, [NotNullWhen(true)] out string? actualFilePath);
    bool FileExists(ReadOnlySpan<char> filePath, bool megFileOnly = false);
    bool FileExists(ReadOnlySpan<char> filePath, bool megFileOnly, out bool pathTooLong);

    Stream? TryOpenFile(string filePath, bool megFileOnly = false);

    Stream? TryOpenFile(ReadOnlySpan<char> filePath, bool megFileOnly = false);
}