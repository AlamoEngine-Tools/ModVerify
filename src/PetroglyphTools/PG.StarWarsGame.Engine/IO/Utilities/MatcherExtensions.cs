using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing;

namespace PG.StarWarsGame.Engine.IO.Utilities;

// Taken from https://github.com/vipentti/Vipentti.IO.Abstractions.FileSystemGlobbing

/// <summary>
/// Provides extensions for <see cref="Matcher" /> to support <see cref="IFileSystem" />
/// </summary>
internal static class MatcherExtensions
{
    /// <summary>
    /// Searches the directory specified for all files matching patterns added to this instance of <see cref="Matcher" />
    /// </summary>
    /// <param name="matcher">The matcher</param>
    /// <param name="fileSystem">The filesystem</param>
    /// <param name="directoryPath">The root directory for the search</param>
    /// <returns>Always returns instance of <see cref="PatternMatchingResult" />, even if no files were matched</returns>
    public static PatternMatchingResult Execute(this Matcher matcher, IFileSystem fileSystem, string directoryPath)
    {
        if (matcher == null) 
            throw new ArgumentNullException(nameof(matcher));
        if (fileSystem == null)
            throw new ArgumentNullException(nameof(fileSystem));
        return Execute(matcher, fileSystem, fileSystem.DirectoryInfo.New(directoryPath));
    }

    /// <inheritdoc cref="Execute(Matcher, IFileSystem, string)"/>
    public static PatternMatchingResult Execute(this Matcher matcher, IFileSystem fileSystem, IDirectoryInfo directoryInfo)
    {
        if (matcher == null) 
            throw new ArgumentNullException(nameof(matcher));
        if (fileSystem == null) 
            throw new ArgumentNullException(nameof(fileSystem));
        if (directoryInfo == null)
            throw new ArgumentNullException(nameof(directoryInfo));
        return matcher.Execute(new DirectoryInfoGlobbingWrapper(fileSystem, directoryInfo));
    }

    /// <summary>
    /// Searches the directory specified for all files matching patterns added to this instance of <see cref="Matcher" />
    /// </summary>
    /// <param name="matcher">The matcher</param>
    /// <param name="fileSystem">The filesystem</param>
    /// <param name="directoryPath">The root directory for the search</param>
    /// <returns>Absolute file paths of all files matched. Empty enumerable if no files matched given patterns.</returns>
    public static IEnumerable<string> GetResultsInFullPath(this Matcher matcher, IFileSystem fileSystem, string directoryPath)
    {
        if (matcher == null) 
            throw new ArgumentNullException(nameof(matcher));
        if (fileSystem == null) 
            throw new ArgumentNullException(nameof(fileSystem));
        return GetResultsInFullPath(matcher, fileSystem, fileSystem.DirectoryInfo.New(directoryPath));
    }

    /// <inheritdoc cref="GetResultsInFullPath(Matcher, IFileSystem, string)"/>
    public static IEnumerable<string> GetResultsInFullPath(this Matcher matcher, IFileSystem fileSystem, IDirectoryInfo directoryInfo)
    {
        var matches = Execute(matcher, fileSystem, directoryInfo);

        if (!matches.HasMatches)
            return Enumerable.Empty<string>();
        
        var fsPath = fileSystem.Path;
        var directoryFullName = directoryInfo.FullName;

        return matches.Files.Select(GetFullPath);

        string GetFullPath(FilePatternMatch match)
        {
            return fsPath.GetFullPath(fsPath.Combine(directoryFullName, match.Path));
        }
    }
}