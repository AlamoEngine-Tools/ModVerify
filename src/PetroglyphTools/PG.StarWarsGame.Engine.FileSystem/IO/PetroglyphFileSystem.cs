using AnakinRaW.CommonUtilities.FileSystem.Normalization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;

namespace PG.StarWarsGame.Engine.IO;

/// <summary>
/// A file system abstraction for the Petroglyph game engine.
/// </summary>
/// <remarks>
/// This file system abstraction simulates Windows-like behavior for all its public methods on Linux,
/// such as correct handling of paths containing backslashes ("\"). On Windows itself the behavior is unchanged.
/// </remarks>
public sealed partial class PetroglyphFileSystem
{
    private const char DirectorySeparatorChar = '/';
    private const char AltDirectorySeparatorChar = '\\';

    // ReSharper disable once InconsistentNaming
    private static readonly PathNormalizeOptions PGFileSystemDirectorySeparatorNormalizeOptions = new()
    {
        TreatBackslashAsSeparator = true, // Ensure that we treat backslashes as separators on Linux
        UnifyDirectorySeparators = true,
        UnifySeparatorKind = DirectorySeparatorKind.System
    };

    /// <summary>
    /// Gets the underlying file system abstraction.
    /// </summary>
    public IFileSystem UnderlyingFileSystem { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PetroglyphFileSystem"/> class.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> used to resolve dependencies required by the file system.</param>
    /// <exception cref="ArgumentNullException"><paramref name="serviceProvider"/> is <see langword="null"/>.</exception>
    public PetroglyphFileSystem(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));
        UnderlyingFileSystem = serviceProvider.GetRequiredService<IFileSystem>();

        Strategy = CreateDefaultStrategy();
    }
    
    /// <summary>
    /// Determines whether the specified path ends with a directory separator character.
    /// </summary>
    /// <param name="path">The path to check for a trailing directory separator.</param>
    /// <returns>
    /// <see langword="true"/> if the path ends with a directory separator character; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method always considers both <c>'/'</c> and <c>'\\'</c> as valid directory separator characters.
    /// </remarks>
    public bool HasTrailingDirectorySeparator(ReadOnlySpan<char> path)
    {
        return path.Length > 0 && IsDirectorySeparator(path[path.Length - 1]);
    }
    
    internal FileSystemStream OpenRead(string filePath)
    {
        return UnderlyingFileSystem.FileStream.New(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
    
    private static bool IsPathRooted(ReadOnlySpan<char> path)
    {
        // The original implementation, obviously, also checks for drive signatures (e.g, c:, X:).
        // We don't expect such paths ever when running in linux mode, so we simply ignore these
        var length = path.Length;
        return length >= 1 && IsDirectorySeparator(path[0]);
    }
    
    internal static ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path)
    {
        if (IsEffectivelyEmpty(path))
            return ReadOnlySpan<char>.Empty;

        var pathRoot = GetRootLength(path);
        return pathRoot <= 0 ? ReadOnlySpan<char>.Empty : path.Slice(0, pathRoot);
    }

    /// <summary>
    /// Returns the length of the path's root: 1 for a leading separator (<c>/foo</c>), or 3 for a
    /// Windows-style drive root (<c>C:\foo</c> or <c>C:/foo</c>). 0 otherwise. Both <c>/</c> and <c>\</c>
    /// are accepted as separators so this is safe to call on host-OS paths from either platform.
    /// </summary>
    private static int GetRootLength(ReadOnlySpan<char> path)
    {
        if (path.Length == 0)
            return 0;

        if (IsDirectorySeparator(path[0]))
            return 1;

        if (path.Length >= 3 && IsAsciiLetter(path[0]) && path[1] == ':' && IsDirectorySeparator(path[2]))
            return 3;

        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsAsciiLetter(char c)
    {
        return c is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
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
    internal static bool IsDirectorySeparator(char c)
    {
        return c is DirectorySeparatorChar or AltDirectorySeparatorChar;
    }
}