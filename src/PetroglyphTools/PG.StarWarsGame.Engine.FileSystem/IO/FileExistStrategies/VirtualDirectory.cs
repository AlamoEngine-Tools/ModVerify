using System.Collections.Generic;

namespace PG.StarWarsGame.Engine.IO.FileExistStrategies;

/// <summary>
/// Immutable snapshot of a single directory's file listing. Files only — no subdirectory recursion.
/// </summary>
internal sealed class VirtualDirectory(string onDiskPath, IReadOnlyDictionary<string, string> files)
{
    /// <summary>Gets the directory's path with the on-disk casing.</summary>
    public string OnDiskPath { get; } = onDiskPath;

    /// <summary>
    /// Gets the filename map.
    /// </summary>
    public IReadOnlyDictionary<string, string> Files { get; } = files;
}
