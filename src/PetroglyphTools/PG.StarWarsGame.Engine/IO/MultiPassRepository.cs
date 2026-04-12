using PG.StarWarsGame.Engine.IO.Repositories;
using PG.StarWarsGame.Engine.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace PG.StarWarsGame.Engine.IO;

internal abstract class MultiPassRepository(GameRepository baseRepository) : IRepository
{
    protected readonly GameRepository BaseRepository = baseRepository;
    
    public Stream OpenFile(string filePath, bool megFileOnly = false)
    {
        return OpenFile(filePath.AsSpan(), megFileOnly);
    }

    public Stream OpenFile(ReadOnlySpan<char> filePath, bool megFileOnly = false)
    {
        var fileStream = TryOpenFile(filePath, megFileOnly);
        if (fileStream is null)
            throw new FileNotFoundException($"Unable to find game file: {filePath.ToString()}");
        return fileStream;
    }

    public bool FileExists(string filePath, bool megFileOnly = false)
    {
        return FileExists(filePath.AsSpan(), megFileOnly);
    }

    public bool FileExists(string filePath, bool megFileOnly, out bool inMeg, [NotNullWhen(true)] out string? actualFilePath)
    {
        var multiPassSb = new ValueStringBuilder(stackalloc char[PGConstants.MaxMegEntryPathLength]);
        var destinationSb = new ValueStringBuilder(stackalloc char[PGConstants.MaxMegEntryPathLength]);
        var result = MultiPassAction(filePath, ref multiPassSb, ref destinationSb, megFileOnly);
        var fileFound = result.FileFound;
        inMeg = result.InMeg;
        if (!fileFound)
            actualFilePath = null;
        else
            actualFilePath = result.InMeg ? result.MegDataEntryReference.Path : result.FilePath.ToString();
        multiPassSb.Dispose();
        destinationSb.Dispose();
        return fileFound;
    }

    public bool FileExists(ReadOnlySpan<char> filePath, bool megFileOnly = false)
    {
        return FileExists(filePath, megFileOnly, out _);
    }

    public bool FileExists(ReadOnlySpan<char> filePath, bool megFileOnly, out bool pathTooLong)
    {
        var multiPassSb = new ValueStringBuilder(stackalloc char[PGConstants.MaxMegEntryPathLength]);
        var destinationSb = new ValueStringBuilder(stackalloc char[PGConstants.MaxMegEntryPathLength]);
        var result = MultiPassAction(filePath, ref multiPassSb, ref destinationSb, megFileOnly);
        var fileFound = result.FileFound;
        pathTooLong = result.PathTooLong;
        multiPassSb.Dispose();
        destinationSb.Dispose();
        return fileFound;
    }

    private protected abstract FileFoundInfo MultiPassAction(
        ReadOnlySpan<char> filePath, 
        ref ValueStringBuilder reusableStringBuilder,
        ref ValueStringBuilder destination,
        bool megFileOnly);

    public Stream? TryOpenFile(string filePath, bool megFileOnly = false)
    {
        return TryOpenFile(filePath.AsSpan(), megFileOnly);
    }

    public Stream? TryOpenFile(ReadOnlySpan<char> filePath, bool megFileOnly = false)
    {
        var multiPassSb = new ValueStringBuilder(stackalloc char[PGConstants.MaxMegEntryPathLength]);
        var destinationSb = new ValueStringBuilder(stackalloc char[PGConstants.MaxMegEntryPathLength]);
        var fileFound = MultiPassAction(filePath, ref multiPassSb, ref destinationSb, megFileOnly);
        var result = BaseRepository.OpenFileCore(fileFound);
        multiPassSb.Dispose();
        destinationSb.Dispose();
        return result;
    }
}