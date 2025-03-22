using System;
using System.Runtime.CompilerServices;
using PG.StarWarsGame.Engine.IO.Utilities;
using PG.StarWarsGame.Engine.Utilities;
#if NETSTANDARD2_0 || NETFRAMEWORK
using AnakinRaW.CommonUtilities.FileSystem;
#endif

namespace PG.StarWarsGame.Engine.IO.Repositories;

internal class ModelRepository(GameRepository baseRepository, IServiceProvider serviceProvider)
    : MultiPassRepository(baseRepository, serviceProvider)
{
    private protected override FileFoundInfo MultiPassAction(
        ReadOnlySpan<char> filePath, 
        ref ValueStringBuilder reusableStringBuilder,
        ref ValueStringBuilder destination, 
        bool megFileOnly)
    {
        if (!IsValidSize(filePath))
            return default;

        var fileInfo = BaseRepository.FindFile(filePath, ref destination, megFileOnly);
        if (fileInfo.FileFound)
            return fileInfo;
        
        destination.Length = 0;

        var stripped = StripFileName(filePath);

        var path = FileSystem.Path.GetDirectoryName(filePath);
        FileSystem.Path.Join(path, stripped, ref reusableStringBuilder);
        reusableStringBuilder.Append(".ALO");

        var alternatePath = reusableStringBuilder.AsSpan();
        return !IsValidSize(alternatePath)
            ? default
            : BaseRepository.FindFile(alternatePath, ref destination, megFileOnly);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidSize(ReadOnlySpan<char> path)
    {
        return path.Length != 0 && path.Length < PGConstants.MaxModelFileName;
    }

    private static ReadOnlySpan<char> StripFileName(ReadOnlySpan<char> src)
    {
        var tmp = src;

        for (var i = src.Length - 1; i >= 0; --i)
        {
            if (src[i] == '.')
                tmp = src.Slice(0, i);

            if (src[i] == '/' || src[i] == '\\')
                return tmp.Slice(i + 1);
        }

        return tmp;
    }
}