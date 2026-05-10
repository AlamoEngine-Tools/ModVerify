using System;
using System.IO.Abstractions;
using PG.StarWarsGame.Engine.Utilities;

namespace PG.StarWarsGame.Engine.IO.FileExistStrategies;

internal abstract class FileExistsStrategy(IFileSystem fileSystem) : IDisposable
{
    protected readonly IFileSystem FileSystem = fileSystem;

    public abstract bool FileExists(ReadOnlySpan<char> baseDirectory, ref ValueStringBuilder stringBuilder);

    public virtual void Dispose() { }
}
