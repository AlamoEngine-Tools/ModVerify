using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PG.StarWarsGame.Engine.IO;

namespace PG.StarWarsGame.Engine.Testing;

/// <summary>Helpers for selecting <see cref="PetroglyphFileSystem"/> file-exists strategies in tests.</summary>
public static class PetroglyphFileSystemTestHelpers
{
    /// <summary>The file-exists strategies supported on the current OS (Windows-backed strategies are Windows-only).</summary>
    public static IReadOnlyList<PetroglyphFileSystemStrategy> SupportedForCurrentOS()
    {
        var strategies = new List<PetroglyphFileSystemStrategy>
        {
            PetroglyphFileSystemStrategy.Wine,
            PetroglyphFileSystemStrategy.VirtualWineFallback,
            PetroglyphFileSystemStrategy.LiveVirtualWineFallback,
        };
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            strategies.Add(PetroglyphFileSystemStrategy.Windows);
            strategies.Add(PetroglyphFileSystemStrategy.VirtualWindowsFallback);
            strategies.Add(PetroglyphFileSystemStrategy.LiveVirtualWindowsFallback);
        }
        return strategies;
    }

    /// <summary>Switches <paramref name="fileSystem"/> to the given file-exists strategy.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="fileSystem"/> is <see langword="null"/>.</exception>
    /// <exception cref="PlatformNotSupportedException">A Windows-backed strategy is selected on a non-Windows host.</exception>
    public static void ApplyStrategy(this PetroglyphFileSystem fileSystem, PetroglyphFileSystemStrategy strategy)
    {
        if (fileSystem is null)
            throw new ArgumentNullException(nameof(fileSystem));

        switch (strategy)
        {
            case PetroglyphFileSystemStrategy.Windows: fileSystem.UseWindowsStrategy(); break;
            case PetroglyphFileSystemStrategy.Wine: fileSystem.UseWineStrategy(); break;
            case PetroglyphFileSystemStrategy.VirtualWindowsFallback: fileSystem.UseVirtualStrategy(windowsFallback: true); break;
            case PetroglyphFileSystemStrategy.VirtualWineFallback: fileSystem.UseVirtualStrategy(windowsFallback: false); break;
            case PetroglyphFileSystemStrategy.LiveVirtualWindowsFallback: fileSystem.UseLiveVirtualStrategy(windowsFallback: true); break;
            case PetroglyphFileSystemStrategy.LiveVirtualWineFallback: fileSystem.UseLiveVirtualStrategy(windowsFallback: false); break;
            default: throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
        }
    }
}