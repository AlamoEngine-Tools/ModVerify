using System;
using PG.Commons.Utilities;

namespace PG.StarWarsGame.Engine.Repositories;

internal class TextureRepository(GameRepository baseRepository, IServiceProvider serviceProvider) : MultiPassRepository(baseRepository, serviceProvider)
{
    private static readonly string DdsExtension = ".dds";
    private static readonly string TexturePath = "DATA\\ART\\TEXTURES\\";


    private protected override FileFoundInfo MultiPassAction(
        ReadOnlySpan<char> filePath, 
        ref ValueStringBuilder multiPassStringBuilder, 
        ref ValueStringBuilder filePathStringBuilder, 
        bool megFileOnly)
    {
        if (filePath.Length > PGConstants.MaxPathLength)
            return default;

        multiPassStringBuilder.Append(filePath);

        var foundInfo = FindTexture(ref multiPassStringBuilder, ref filePathStringBuilder);

        if (foundInfo.FileFound)
            return foundInfo;

        multiPassStringBuilder.Length = 0;
        multiPassStringBuilder.Append(filePath);

        ChangeExtensionTo(ref multiPassStringBuilder, DdsExtension.AsSpan());

        return FindTexture(ref multiPassStringBuilder, ref filePathStringBuilder);
    }

    private FileFoundInfo FindTexture(ref ValueStringBuilder multiPassStringBuilder, ref ValueStringBuilder pathStringBuilder)
    {
        var fileInfo = BaseRepository.FindFile(multiPassStringBuilder.AsSpan(), ref pathStringBuilder);
        if (fileInfo.FileFound)
            return fileInfo;


        // Only PG knows why they only search for backslash and not also forward slash,
        // when in fact in other methods, they handle both. 
        var separatorIndex = multiPassStringBuilder.AsSpan().LastIndexOf('\\');
        if (separatorIndex != -1)
            multiPassStringBuilder.Remove(0 , separatorIndex + 1);

        multiPassStringBuilder.Insert(0, TexturePath);

        return BaseRepository.FindFile(multiPassStringBuilder.AsSpan(), ref pathStringBuilder);
    }

    private static void ChangeExtensionTo(ref ValueStringBuilder stringBuilder, ReadOnlySpan<char> extension)
    {
        // We cannot use Path.ChangeExtension as the PG implementation supports some strange things
        // like that a string "c:\\file.tga\\" ending with a directory separator. The PG result will be 
        // "c:\\file.dds" while Path.ChangeExtension would return "c:\\file.tga\\.dds"

        // Also, while there are many cases, where this method breaks (such as "c:/test.abc/path.dds"),
        // it's the way how the engine works 🤷‍
        var firstPeriod = stringBuilder.AsSpan().IndexOf('.');
        if (firstPeriod == -1)
            return;

        stringBuilder.Length = firstPeriod;
        stringBuilder.Append(extension);
    }
}