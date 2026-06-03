using PG.StarWarsGame.Engine.IO;

namespace PG.StarWarsGame.Engine.Testing;

/// <summary>
/// A selectable <see cref="PetroglyphFileSystem"/> file-exists strategy. The Windows-backed strategies are
/// only supported on Windows hosts.
/// </summary>
public enum PetroglyphFileSystemStrategy
{
    /// <summary>Win32 <c>CreateFileA</c> per lookup (Windows only).</summary>
    Windows,

    /// <summary>Case-folding, component-by-component walk.</summary>
    Wine,

    /// <summary>Game-directory snapshot, delegating outside-game lookups to the Windows strategy (Windows only).</summary>
    VirtualWindowsFallback,

    /// <summary>Game-directory snapshot, delegating outside-game lookups to the Wine strategy.</summary>
    VirtualWineFallback,

    /// <summary>Watcher-backed game-directory snapshot, with the Windows fallback (Windows only).</summary>
    LiveVirtualWindowsFallback,

    /// <summary>Watcher-backed game-directory snapshot, with the Wine fallback.</summary>
    LiveVirtualWineFallback,
}