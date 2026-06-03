using System;
using System.Runtime.CompilerServices;
using PG.StarWarsGame.Engine.Utilities;

namespace PG.StarWarsGame.Engine.IO.Repositories;

internal class ModelRepository(GameRepository baseRepository) : MultiPassRepository(baseRepository)
{
    private protected override FileFoundInfo MultiPassAction(
        ReadOnlySpan<char> filePath, 
        ref ValueStringBuilder reusableStringBuilder,
        ref ValueStringBuilder destination, 
        bool megFileOnly)
    {
        if (!IsValidSize(filePath, out var pathTooLong))
            return new FileFoundInfo { PathTooLong = pathTooLong };

        var fileInfo = BaseRepository.FindFile(filePath, ref destination, megFileOnly);
        if (fileInfo.FileFound)
            return fileInfo;
        
        destination.Length = 0;

        var stripped = StripFileName(filePath);

        var path = BaseRepository.PGFileSystem.GetDirectoryName(filePath);
        BaseRepository.PGFileSystem.JoinPath(path, stripped, ref reusableStringBuilder);
        reusableStringBuilder.Append(".ALO");

        var alternatePath = reusableStringBuilder.AsSpan();
        return !IsValidSize(alternatePath, out pathTooLong)
            ? new FileFoundInfo { PathTooLong = pathTooLong }
            : BaseRepository.FindFile(alternatePath, ref destination, megFileOnly);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidSize(ReadOnlySpan<char> path, out bool pathTooLong)
    { 
        switch (path.Length)
        {
            case 0:
                pathTooLong = false;
                return false;
            case >= PGConstants.MaxModelFileName:
                pathTooLong = true;
                return false;
            default:
                pathTooLong = false;
                return true;
        }
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