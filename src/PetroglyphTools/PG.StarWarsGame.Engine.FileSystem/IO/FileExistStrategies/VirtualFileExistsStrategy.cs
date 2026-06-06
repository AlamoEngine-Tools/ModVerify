using System.IO.Abstractions;

namespace PG.StarWarsGame.Engine.IO.FileExistStrategies;

internal sealed class VirtualFileExistsStrategy(IFileSystem fileSystem, FileExistsStrategy underlying)
    : VirtualFileExistsStrategyBase(fileSystem, underlying);
