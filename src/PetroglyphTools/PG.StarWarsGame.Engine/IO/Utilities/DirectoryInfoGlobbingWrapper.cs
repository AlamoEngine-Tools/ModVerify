using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace PG.StarWarsGame.Engine.IO.Utilities;

// Taken from https://github.com/vipentti/Vipentti.IO.Abstractions.FileSystemGlobbing

/// <summary>
/// Wraps <see cref="IDirectoryInfo" /> to be used with <see cref="Microsoft.Extensions.FileSystemGlobbing.Abstractions.DirectoryInfoBase"/>
/// </summary>
internal sealed class DirectoryInfoGlobbingWrapper : Microsoft.Extensions.FileSystemGlobbing.Abstractions.DirectoryInfoBase
{
    private readonly IFileSystem _fileSystem;
    private readonly IDirectoryInfo _directoryInfo;
    private readonly bool _isParentPath;

    /// <inheritdoc />
    public override string Name => _isParentPath ? ".." : _directoryInfo.Name;

    /// <inheritdoc />
    public override string FullName => _directoryInfo.FullName;

    /// <inheritdoc />
    public override Microsoft.Extensions.FileSystemGlobbing.Abstractions.DirectoryInfoBase? ParentDirectory =>
        _directoryInfo.Parent is null
            ? null
            : new DirectoryInfoGlobbingWrapper(_fileSystem, _directoryInfo.Parent);

    /// <summary>
    /// Construct a new instance of <see cref="DirectoryInfoGlobbingWrapper" />
    /// </summary>
    /// <param name="fileSystem">The filesystem</param>
    /// <param name="directoryInfo">The directory</param>
    public DirectoryInfoGlobbingWrapper(IFileSystem fileSystem, IDirectoryInfo directoryInfo)
        : this(fileSystem, directoryInfo, isParentPath: false)
    {
    }

    private DirectoryInfoGlobbingWrapper(IFileSystem fileSystem, IDirectoryInfo directoryInfo, bool isParentPath)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _directoryInfo = directoryInfo ?? throw new ArgumentNullException(nameof(directoryInfo));
        _isParentPath = isParentPath;
    }

    /// <inheritdoc />
    public override IEnumerable<Microsoft.Extensions.FileSystemGlobbing.Abstractions.FileSystemInfoBase> EnumerateFileSystemInfos()
    {
        if (_directoryInfo.Exists)
        {
            IEnumerable<IFileSystemInfo> fileSystemInfos;
            try
            {
                fileSystemInfos = _directoryInfo.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly);
            }
            catch (DirectoryNotFoundException)
            {
                yield break;
            }

            foreach (var fileSystemInfo in fileSystemInfos)
            {
                yield return fileSystemInfo switch
                {
                    IDirectoryInfo directoryInfo => new DirectoryInfoGlobbingWrapper(_fileSystem, directoryInfo),
                    IFileInfo fileInfo => new FileInfoGlobbingWrapper(_fileSystem, fileInfo),
                    _ => throw new NotSupportedException()
                };
            }
        }
    }

    /// <inheritdoc />
    public override Microsoft.Extensions.FileSystemGlobbing.Abstractions.DirectoryInfoBase? GetDirectory(string path)
    {
        var isParentPath = string.Equals(path, "..", StringComparison.Ordinal);

        if (isParentPath)
        {
            return new DirectoryInfoGlobbingWrapper(_fileSystem,
                _fileSystem.DirectoryInfo.New(Path.Combine(_directoryInfo.FullName, path)), isParentPath);
        }

        var dirs = _directoryInfo.GetDirectories(path);

        return dirs switch
        {
            { Length: 1 }
                => new DirectoryInfoGlobbingWrapper(_fileSystem, dirs[0], isParentPath),
            { Length: 0 } => null,
            // This shouldn't happen. The parameter name isn't supposed to contain wild card.
            _
                => throw new InvalidOperationException(
                    $"More than one sub directories are found under {_directoryInfo.FullName} with name {path}."
                ),
        };
    }

    /// <inheritdoc />
    public override Microsoft.Extensions.FileSystemGlobbing.Abstractions.FileInfoBase GetFile(string path)
    {
        return new FileInfoGlobbingWrapper(_fileSystem, _fileSystem.FileInfo.New(Path.Combine(FullName, path)));
    }
}