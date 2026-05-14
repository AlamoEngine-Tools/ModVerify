using System;
using System.Runtime.InteropServices;
using PG.StarWarsGame.Engine.IO.FileExistStrategies;

namespace PG.StarWarsGame.Engine.IO;

public sealed partial class PetroglyphFileSystem
{
    private FileExistsStrategy _strategy;

    internal void CleanupStrategy() => _strategy.Cleanup();

    private FileExistsStrategy CreateDefaultStrategy()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new WindowsFileExistsStrategy(_underlyingFileSystem)
            : new VirtualFileExistsStrategy(_underlyingFileSystem, new WineFileExistsStrategy(_underlyingFileSystem));
    }

    /// <summary>
    /// Switches the active file-exists strategy to one that issues a Win32 <c>CreateFileA</c> call per lookup.
    /// </summary>
    /// <remarks>
    /// Supported on Windows hosts only. Each call re-stats the file with no caching.
    /// </remarks>
    /// <exception cref="PlatformNotSupportedException">The host is not Windows.</exception>
    public void UseWindowsStrategy() => SwapStrategy(CreateWindowsStrategy());

    /// <summary>
    /// Switches the active file-exists strategy to a case-folding, component-by-component walk.
    /// </summary>
    /// <remarks>
    /// <note type="warning">
    /// Selecting this strategy directly is rarely correct. Prefer
    /// <see cref="UseVirtualStrategy(bool?)"/> on non-Windows hosts and <see cref="UseWindowsStrategy"/>
    /// on Windows. This method exists primarily to support the search engine used internally by
    /// <see cref="UseVirtualStrategy(bool?)"/> for paths outside the game directory.
    /// </note>
    /// <para>Provides full mediation: every lookup re-walks the path with no caching.</para>
    /// </remarks>
    public void UseWineStrategy() => SwapStrategy(new WineFileExistsStrategy(_underlyingFileSystem));

    /// <summary>
    /// Switches the active file-exists strategy to an immutable per-directory snapshot scoped to the game directory.
    /// </summary>
    /// <remarks>
    /// Lookups under the game directory are answered from a directory snapshot taken on first
    /// access. Lookups outside the game directory delegate to an underlying strategy.
    /// </remarks>
    /// <param name="windowsFallback">
    /// <see langword="true" /> to delegate outside-game-directory lookups to the Windows
    /// <c>CreateFileA</c> strategy; <see langword="false" /> to delegate them to the Wine search
    /// engine; <see langword="null" /> to pick the Windows strategy on Windows hosts and the Wine
    /// strategy otherwise.
    /// </param>
    /// <exception cref="PlatformNotSupportedException">
    /// <paramref name="windowsFallback"/> is <see langword="true" /> and the host is not Windows.
    /// </exception>
    public void UseVirtualStrategy(bool? windowsFallback = null)
    {
        var useWindows = windowsFallback ?? RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        FileExistsStrategy fallback = useWindows
            ? CreateWindowsStrategy()
            : new WineFileExistsStrategy(_underlyingFileSystem);
        UseVirtualStrategy(fallback);
    }

    internal void UseVirtualStrategy(FileExistsStrategy underlying)
        => SwapStrategy(new VirtualFileExistsStrategy(_underlyingFileSystem, underlying));

    /// <summary>
    /// Switches the active file-exists strategy to a snapshot-based one that refreshes itself when
    /// files are added, removed, or renamed in the game directory.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Equivalent to <see cref="UseVirtualStrategy(bool?)"/> for lookups, but lazily attaches a
    /// recursive <see cref="System.IO.FileSystemWatcher"/> to every distinct base directory passed
    /// to <see cref="LiveVirtualFileExistsStrategy.FileExists"/>. Each watcher's events invalidate cached directory listings under its
    /// root on demand; the next lookup rebuilds the affected snapshot from disk. File
    /// <em>content</em> changes are not tracked.
    /// </para>
    /// <para>
    /// Each watcher is created on the first lookup that lands inside its base directory and is torn
    /// down when the strategy is replaced or the file system is disposed. If a watcher's internal
    /// buffer overflows or the OS otherwise reports an error, only that watcher is removed and only
    /// its subtree is evicted from the cache; other roots continue to be tracked.
    /// </para>
    /// <para>
    /// On Linux, each watcher consumes one inotify slot per directory in its subtree (per-user
    /// kernel limit, <c>fs.inotify.max_user_watches</c>). Consumers tracking many large trees may
    /// need to raise this limit.
    /// </para>
    /// </remarks>
    /// <param name="windowsFallback">
    /// <see langword="true" /> to delegate outside-game-directory lookups to the Windows
    /// strategy; <see langword="false" /> to delegate them to the Wine search
    /// engine; <see langword="null" /> to pick the Windows strategy on Windows hosts and the Wine
    /// strategy otherwise.
    /// </param>
    /// <exception cref="PlatformNotSupportedException">
    /// <paramref name="windowsFallback"/> is <see langword="true" /> and the host is not Windows.
    /// </exception>
    public void UseLiveVirtualStrategy(bool? windowsFallback = null)
    {
        var useWindows = windowsFallback ?? RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        FileExistsStrategy fallback = useWindows
            ? CreateWindowsStrategy()
            : new WineFileExistsStrategy(_underlyingFileSystem);
        UseLiveVirtualStrategy(fallback);
    }

    internal void UseLiveVirtualStrategy(FileExistsStrategy underlying)
        => SwapStrategy(new LiveVirtualFileExistsStrategy(_underlyingFileSystem, underlying));

    private WindowsFileExistsStrategy CreateWindowsStrategy()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            throw new PlatformNotSupportedException(
                "The Windows file-exists strategy relies on Win32 CreateFileA and is only supported on Windows hosts.");
        return new WindowsFileExistsStrategy(_underlyingFileSystem);
    }

    private void SwapStrategy(FileExistsStrategy next)
    {
        var old = _strategy;
        _strategy = next;
        old.Cleanup();
    }
}
