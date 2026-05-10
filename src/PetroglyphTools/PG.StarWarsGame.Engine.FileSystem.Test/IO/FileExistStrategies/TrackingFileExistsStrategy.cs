using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using PG.StarWarsGame.Engine.IO.FileExistStrategies;
using PG.StarWarsGame.Engine.Utilities;

namespace PG.StarWarsGame.Engine.FileSystem.Test.IO.FileExistStrategies;

internal sealed class TrackingFileExistsStrategy(IFileSystem fileSystem) : FileExistsStrategy(fileSystem)
{
    public int CallCount { get; private set; }

    public List<string> InvokedPaths { get; } = [];

    public bool ReturnValue { get; set; }

    public string? ResolvedPath { get; set; }

    public override bool FileExists(ReadOnlySpan<char> gameDirectory, ref ValueStringBuilder stringBuilder)
    {
        CallCount++;
        InvokedPaths.Add(stringBuilder.AsSpan().ToString());
        if (ReturnValue && ResolvedPath is not null)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(ResolvedPath);
        }
        return ReturnValue;
    }
}
