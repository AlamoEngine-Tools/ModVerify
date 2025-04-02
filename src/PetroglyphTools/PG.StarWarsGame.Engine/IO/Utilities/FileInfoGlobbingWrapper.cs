using System.IO.Abstractions;

namespace PG.StarWarsGame.Engine.IO.Utilities;

// Taken from https://github.com/vipentti/Vipentti.IO.Abstractions.FileSystemGlobbing

internal sealed class FileInfoGlobbingWrapper
    : Microsoft.Extensions.FileSystemGlobbing.Abstractions.FileInfoBase
{
    private readonly IFileSystem _fileSystem;
    private readonly IFileInfo _fileInfo;

    /// <inheritdoc />
    public override string Name => _fileInfo.Name;

    /// <inheritdoc />
    public override string FullName => _fileInfo.FullName;

    /// <inheritdoc />
    public override Microsoft.Extensions.FileSystemGlobbing.Abstractions.DirectoryInfoBase? ParentDirectory =>
        _fileInfo.Directory is null
            ? null
            : new DirectoryInfoGlobbingWrapper(_fileSystem, _fileInfo.Directory);

    /// <summary>
    /// InitializeAsync a new instance
    /// </summary>
    /// <param name="fileSystem">The filesystem</param>
    /// <param name="fileInfo">The file</param>
    public FileInfoGlobbingWrapper(IFileSystem fileSystem, IFileInfo fileInfo)
    {
        _fileSystem = fileSystem;
        _fileInfo = fileInfo;
    }
}