using System;
using System.Runtime.InteropServices;
using PG.StarWarsGame.Engine.IO.FileExistStrategies;

namespace PG.StarWarsGame.Engine.IO;

public sealed partial class PetroglyphFileSystem
{
    private FileExistsStrategy _strategy;

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
    /// <see cref="UseVirtualStrategy"/> on non-Windows hosts and <see cref="UseWindowsStrategy"/>
    /// on Windows. This method exists primarily to support the search engine used internally by
    /// <see cref="UseVirtualStrategy"/> for paths outside the game directory.
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
    /// engine, which works on every host.
    /// </param>
    /// <exception cref="PlatformNotSupportedException">
    /// <paramref name="windowsFallback"/> is <see langword="true" /> and the host is not Windows.
    /// </exception>
    public void UseVirtualStrategy(bool windowsFallback = false)
    {
        FileExistsStrategy fallback = windowsFallback
            ? CreateWindowsStrategy()
            : new WineFileExistsStrategy(_underlyingFileSystem);
        UseVirtualStrategy(fallback);
    }

    internal void UseVirtualStrategy(FileExistsStrategy underlying)
        => SwapStrategy(new VirtualFileExistsStrategy(_underlyingFileSystem, underlying));

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
        old?.Dispose();
    }
}
