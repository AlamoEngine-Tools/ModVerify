using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PG.StarWarsGame.Engine.Repositories;

public class EffectsRepository(IGameRepository baseRepository, IServiceProvider serviceProvider) : MultiPassRepository(baseRepository, serviceProvider)
{
    private static readonly string[] LookupPaths =
    [
        "Data\\Art\\Shaders",
        "Data\\Art\\Shaders\\Terrain",
        "Data\\Art\\Shaders\\Engine",
    ];

    private static readonly string[] ShaderExtensions = [".fx", ".fxo", ".fxh"];

    [return:MaybeNull]
    protected override T MultiPassAction<T>(string inputPath, Func<string, (bool success, T result)> fileAction)
    {
        var currExt = FileSystem.Path.GetExtension(inputPath);
        if (!ShaderExtensions.Contains(currExt, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException("Invalid data extension for shader. Must be .fx, .fxh or .fxo", nameof(inputPath));

        foreach (var directory in LookupPaths)
        {
            var lookupPath = FileSystem.Path.Combine(directory, inputPath);

            foreach (var ext in ShaderExtensions)
            {
                lookupPath = FileSystem.Path.ChangeExtension(lookupPath, ext);

                var actionResult = fileAction(lookupPath);
                if (actionResult.success)
                    return actionResult.result;
            }
        }

        return default;
    }
}