using System;
using System.IO;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace PG.StarWarsGame.Engine.Repositories;

public abstract class MultiPassRepository(IGameRepository baseRepository, IServiceProvider serviceProvider) : IRepository
{
    protected readonly IFileSystem FileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    public Stream OpenFile(string filePath, bool megFileOnly = false)
    {
        var fileStream = TryOpenFile(filePath, megFileOnly);
        if (fileStream is null)
            throw new FileNotFoundException($"Unable to find game file: {filePath}");
        return fileStream;
    }

    public bool FileExists(string filePath, bool megFileOnly = false)
    {
        return MultiPassAction(filePath, actualPath => baseRepository.FileExists(actualPath, megFileOnly));
    }

    public Stream? TryOpenFile(string filePath, bool megFileOnly = false)
    {
        return MultiPassAction<Stream?>(filePath, path =>
        {
            var stream = baseRepository.TryOpenFile(path, megFileOnly);
            if (stream is null)
                return (false, null);
            return (true, stream);
        });
    }

    protected abstract T? MultiPassAction<T>(string inputPath, Func<string, (bool success, T result)> fileAction);

    protected bool MultiPassAction(string inputPath, Predicate<string> fileAction)
    {
        return MultiPassAction(inputPath, path =>
        {
            var result = fileAction(path);
            return (result, result);
        });
    }
}