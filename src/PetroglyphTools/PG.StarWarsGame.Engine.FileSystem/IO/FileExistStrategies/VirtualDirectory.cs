using System.Collections.Generic;

namespace PG.StarWarsGame.Engine.IO.FileExistStrategies;

/// <summary>
/// Immutable snapshot of a single directory's file listing. Files only — no subdirectory recursion.
/// Built once by <see cref="VirtualFileExistsStrategy"/> and never mutated thereafter.
/// </summary>
internal sealed class VirtualDirectory(string onDiskPath, Dictionary<string, string> files)
{
    /// <summary>The directory's path with the on-disk casing.</summary>
    public string OnDiskPath { get; } = onDiskPath;

    /// <summary>
    /// Filename map. Keys compare case-insensitively (so callers can look up "FOO.XML" against
    /// the on-disk "foo.xml") and the value carries the case-preserved on-disk filename used
    /// when joining the result back into a full path.
    /// </summary>
    public Dictionary<string, string> Files { get; } = files;
}
