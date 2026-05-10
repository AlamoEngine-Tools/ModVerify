using System;
using AnakinRaW.CommonUtilities.Testing;
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Engine.Utilities;

namespace PG.StarWarsGame.Engine.FileSystem.Test;

public abstract class TestBaseWithPGFileSystem : TestBaseWithFileSystem
{
    protected PetroglyphFileSystem PgFileSystem { get; }

    protected TestBaseWithPGFileSystem()
    {
        PgFileSystem = new PetroglyphFileSystem(ServiceProvider);
        ConfigureStrategy(PgFileSystem);
    }

    /// <summary>Install the strategy under test on the freshly constructed file system.</summary>
    protected virtual void ConfigureStrategy(PetroglyphFileSystem fs)
    {
        // Use default
    }

    /// <summary>
    /// Test helper that allocates a <see cref="ValueStringBuilder"/>, runs FileExists, and
    /// disposes the buffer. Use when the resolved path is irrelevant to the assertion.
    /// </summary>
    protected bool FileExists(ReadOnlySpan<char> filePath, ReadOnlySpan<char> gameDirectory)
    {
        var sb = new ValueStringBuilder();
        try
        {
            return PgFileSystem.FileExists(filePath, ref sb, gameDirectory);
        }
        finally
        {
            sb.Dispose();
        }
    }
}