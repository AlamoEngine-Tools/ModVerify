using System;
using System.IO;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace PG.StarWarsGame.Engine.IO;

/// <summary>
/// A file system abstraction for the Petroglyph game engine.
/// </summary>
/// <remarks>
/// The file system enforces Windows behavior for all its methods independent of the current operating system.
/// </remarks>
public sealed partial class PetroglyphFileSystem
{
    private const char DirectorySeparatorChar = '/';
    private const char AltDirectorySeparatorChar = '\\';

    private readonly IFileSystem _underlyingFileSystem;
    
    /// <summary>
    /// Gets the underlying file system abstraction.
    /// </summary>
    public IFileSystem UnderlyingFileSystem => _underlyingFileSystem;

    public PetroglyphFileSystem(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));
        _underlyingFileSystem = serviceProvider.GetRequiredService<IFileSystem>();
    }
    
    public bool HasTrailingDirectorySeparator(ReadOnlySpan<char> path)
    {
        return path.Length > 0 && IsDirectorySeparator(path[path.Length - 1]);
    }
    
    internal FileSystemStream OpenRead(string filePath)
    {
        return _underlyingFileSystem.FileStream.New(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
    
    private static bool IsPathRooted(ReadOnlySpan<char> path)
    {
        // The original implementation, of course, also checks for drive signatures (e.g, c:, X:).
        // We don't expect such paths ever when running in linux mode, so we simply ignore these
        var length = path.Length;
        return length >= 1 && IsDirectorySeparator(path[0]);
    }
    
    private static ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path)
    {
        if (IsEffectivelyEmpty(path))
            return ReadOnlySpan<char>.Empty;

        var pathRoot = GetRootLength(path);
        return pathRoot <= 0 ? ReadOnlySpan<char>.Empty : path.Slice(0, pathRoot);
    }

    private static int GetRootLength(ReadOnlySpan<char> path)
    {
        // We don't ever expect drive signatures or UCN paths in a linux environment.
        // Thus, we keep the simple linux check, augmented supporting backslash
        return path.Length > 0 && IsDirectorySeparator(path[0]) ? 1 : 0;
    }
    
    private static bool IsEffectivelyEmpty(ReadOnlySpan<char> path)
    {
        if (path.IsEmpty)
            return true;

        foreach (var c in path)
        {
            if (c != ' ')
                return false;
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsDirectorySeparator(char c)
    {
        return c is '\\' or '/';
    }
}