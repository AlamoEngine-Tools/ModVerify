using System;
using PG.StarWarsGame.Engine.IO.Utilities;
using PG.StarWarsGame.Engine.Utilities;

namespace PG.StarWarsGame.Engine.IO.Repositories;

internal class EffectsRepository(GameRepository baseRepository, IServiceProvider serviceProvider) 
    : MultiPassRepository(baseRepository, serviceProvider)
{
    private static readonly string[] LookupPaths =
    [
        "DATA\\ART\\SHADERS",
        "DATA\\ART\\SHADERS\\TERRAIN",
        // This path is not coded to the engine
        "DATA\\ART\\SHADERS\\ENGINE",
    ];

    // The engine does not support ".fxh" as a shader lookup, but as there might be some pre-compiling going on, this should be OK. 
    private static readonly string[] ShaderExtensions = [".fx", ".fxo", ".fxh"];

    private protected override FileFoundInfo MultiPassAction(
        ReadOnlySpan<char> filePath, 
        ref ValueStringBuilder reusableStringBuilder,
        ref ValueStringBuilder destination,
        bool megFileOnly)
    {
        var strippedName = StripFileName(filePath);

        if (strippedName.Length > PGConstants.MaxEffectFileName)
            return default;

        foreach (var ext in ShaderExtensions)
        {
            var extSpan = ext.AsSpan();

            
            var fileFoundInfo = FindEffect(
                strippedName, 
                extSpan, 
                ReadOnlySpan<char>.Empty,
                ref reusableStringBuilder,
                ref destination);

            if (fileFoundInfo.FileFound)
                return fileFoundInfo;


            foreach (var directory in LookupPaths)
            {
                fileFoundInfo = FindEffect(
                    strippedName,
                    extSpan,
                    directory.AsSpan(),
                    ref reusableStringBuilder,
                    ref destination);

                if (fileFoundInfo.FileFound)
                    return fileFoundInfo;
            }
        }
       

        return default;
    }

    private FileFoundInfo FindEffect(
        ReadOnlySpan<char> strippedName, 
        ReadOnlySpan<char> extension,
        ReadOnlySpan<char> directory, 
        ref ValueStringBuilder multiPassStringBuilder,
        ref ValueStringBuilder filePathStringBuilder)
    {
        multiPassStringBuilder.Length = 0;

        if (directory != ReadOnlySpan<char>.Empty) 
            FileSystem.Path.Join(directory, strippedName, ref multiPassStringBuilder);
        else
            multiPassStringBuilder.Append(strippedName);

        multiPassStringBuilder.Append(extension);

        if (multiPassStringBuilder.Length > PGConstants.MaxEffectFileName)
            return default;

        return BaseRepository.FindFile(multiPassStringBuilder.AsSpan(), ref filePathStringBuilder);
    }

    private static ReadOnlySpan<char> StripFileName(ReadOnlySpan<char> src)
    {
        var destination = src;

        for (var i = src.Length - 1; i >= 0; --i)
        {

            if (src[i] == '.')
                destination = src.Slice(0, i);

            if (src[i] == '/' || src[i] == '\\')
                break;
        }

        return destination;
    }
}