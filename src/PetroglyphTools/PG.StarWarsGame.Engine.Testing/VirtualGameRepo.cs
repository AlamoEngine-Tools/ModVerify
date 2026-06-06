using System;
using System.IO.Abstractions;

namespace PG.StarWarsGame.Engine.Testing;

/// <summary>Represents a disposable temp-directory-backed virtual game repository.</summary>
public sealed class VirtualGameRepo : IDisposable
{
    private readonly IFileSystem _fileSystem;

    /// <summary>Gets the absolute path of the temp directory that backs this repository.</summary>
    public string TempPath { get; }

    /// <summary>Gets the game locations describing the virtual layout.</summary>
    public GameLocations GameLocations { get; }

    /// <summary>Initializes a new instance of the <see cref="VirtualGameRepo"/> class.</summary>
    /// <param name="fileSystem">The file system used to access and clean up the backing directory.</param>
    /// <param name="tempPath">The absolute path of the temp directory that backs this repository.</param>
    /// <param name="gameLocations">The game locations describing the virtual layout.</param>
    /// <exception cref="ArgumentNullException"><paramref name="fileSystem"/>, <paramref name="tempPath"/>, or <paramref name="gameLocations"/> is <see langword="null"/>.</exception>
    public VirtualGameRepo(IFileSystem fileSystem, string tempPath, GameLocations gameLocations)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        TempPath = tempPath ?? throw new ArgumentNullException(nameof(tempPath));
        GameLocations = gameLocations ?? throw new ArgumentNullException(nameof(gameLocations));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_fileSystem.Directory.Exists(TempPath))
            _fileSystem.Directory.Delete(TempPath, recursive: true);
    }
}
