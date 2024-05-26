using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace PG.StarWarsGame.Engine.FileSystem;

public class EffectsRepository(IGameRepository baseRepository, IServiceProvider serviceProvider) : IRepository
{
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    private static readonly string[] LookupPaths =
    [
        "Data\\Art\\Shaders",
        "Data\\Art\\Shaders\\Terrain",
        "Data\\Art\\Shaders\\Engine",
    ];

    private static readonly string[] ShaderExtensions = [".fx", ".fxo", ".fxh"];

    public Stream OpenFile(string filePath, bool megFileOnly = false)
    {
        var shaderStream = TryOpenFile(filePath, megFileOnly);
        if (shaderStream is null)
            throw new FileNotFoundException($"Unable to find game data: {filePath}");
        return shaderStream;
    }

    public bool FileExists(string filePath, bool megFileOnly = false)
    {
        var result = false;
        ShaderFileAction(filePath, shaderPath =>
        {
            if (!baseRepository.FileExists(shaderPath, megFileOnly))
                return false;

            result = true;
            return true;

        });
        return result;
    }

    public Stream? TryOpenFile(string filePath, bool megFileOnly = false)
    {
        Stream? shaderStream = null;
        ShaderFileAction(filePath, shaderPath =>
        {
            var stream = baseRepository.TryOpenFile(shaderPath, megFileOnly);
            if (stream is null)
                return false;
            shaderStream = stream;
            return true;
        });

        return shaderStream;
    }

    private void ShaderFileAction(string filePath, Predicate<string> action)
    {
        var currExt = _fileSystem.Path.GetExtension(filePath);
        if (!ShaderExtensions.Contains(currExt, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException("Invalid data extension for shader. Must be .fx, .fxh or .fxo", nameof(filePath));

        foreach (var directory in LookupPaths)
        {
            var lookupPath = _fileSystem.Path.Combine(directory, filePath);

            foreach (var ext in ShaderExtensions)
            {
                lookupPath = _fileSystem.Path.ChangeExtension(lookupPath, ext);
                if (action(lookupPath))
                    return;
            }
        }
    }
}