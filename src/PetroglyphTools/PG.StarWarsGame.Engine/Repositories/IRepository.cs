using System;
using System.IO;

namespace PG.StarWarsGame.Engine.Repositories;

public interface IRepository
{
    Stream OpenFile(string filePath, bool megFileOnly = false);
    Stream OpenFile(ReadOnlySpan<char> filePath, bool megFileOnly = false);

    bool FileExists(string filePath, bool megFileOnly = false);
    bool FileExists(ReadOnlySpan<char> filePath, bool megFileOnly = false);

    Stream? TryOpenFile(string filePath, bool megFileOnly = false);

    Stream? TryOpenFile(ReadOnlySpan<char> filePath, bool megFileOnly = false);
}