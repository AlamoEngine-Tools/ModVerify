using System;
using System.IO;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.Utilities;

namespace PG.StarWarsGame.Engine.Repositories;

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
        var mpSb = new ValueStringBuilder(stackalloc char[PGConstants.MaxPathLength]);
        var fsp = new ValueStringBuilder(stackalloc char[PGConstants.MaxPathLength]);
        var fileFound = MultiPassAction(filePath, ref mpSb, ref fsp, megFileOnly);
        var result = fileFound.FileFound;
        mpSb.Dispose();
        fsp.Dispose();
        return result;
    }

    private protected abstract FileFoundInfo MultiPassAction(
        ReadOnlySpan<char> filePath, 
        ref ValueStringBuilder multiPassStringBuilder,
        ref ValueStringBuilder filePathStringBuilder,
        bool megFileOnly);

    public Stream? TryOpenFile(string filePath, bool megFileOnly = false)
    {
        return TryOpenFile(filePath.AsSpan(), megFileOnly);
    }

    public Stream? TryOpenFile(ReadOnlySpan<char> filePath, bool megFileOnly = false)
    {
        var mpSb = new ValueStringBuilder(stackalloc char[PGConstants.MaxPathLength]);
        var fsb = new ValueStringBuilder(stackalloc char[PGConstants.MaxPathLength]);
        var fileFound = MultiPassAction(filePath, ref mpSb, ref fsb, megFileOnly);
        var result = BaseRepository.OpenFileCore(fileFound);
        mpSb.Dispose();
        fsb.Dispose();
        return result;
    }
}