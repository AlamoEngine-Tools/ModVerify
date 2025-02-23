using System;
using System.IO;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.IO.Repositories;
using PG.StarWarsGame.Engine.Utilities;

namespace PG.StarWarsGame.Engine.IO;

internal abstract class MultiPassRepository(GameRepository baseRepository, IServiceProvider serviceProvider) : IRepository
{
    protected readonly IFileSystem FileSystem = serviceProvider.GetRequiredService<IFileSystem>();
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

    public bool FileExists(ReadOnlySpan<char> filePath, bool megFileOnly = false)
    {
        var multiPassSb = new ValueStringBuilder(stackalloc char[PGConstants.MaxMegEntryPathLength]);
        var destinationSb = new ValueStringBuilder(stackalloc char[PGConstants.MaxMegEntryPathLength]);
        var fileFound = MultiPassAction(filePath, ref multiPassSb, ref destinationSb, megFileOnly);
        var result = fileFound.FileFound;
        multiPassSb.Dispose();
        destinationSb.Dispose();
        return result;
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