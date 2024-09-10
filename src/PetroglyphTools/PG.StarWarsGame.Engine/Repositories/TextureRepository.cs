using System;

namespace PG.StarWarsGame.Engine.Repositories;

public class TextureRepository(IGameRepository baseRepository, IServiceProvider serviceProvider) : MultiPassRepository(baseRepository, serviceProvider)
{
    protected override T? MultiPassAction<T>(string inputPath, Func<string, (bool success, T result)> fileAction) where T : default
    {
        if (FindTexture(inputPath, fileAction, out var result))
            return result;

        var ddsFilePath = ChangeExtensionTo(inputPath, ".dds");

        if (FindTexture(ddsFilePath, fileAction, out result))
            return result;

        return default;
    }

    private bool FindTexture<T>(string inputPath, Func<string, (bool success, T result)> fileAction, out T? result)
    {
        result = default;
        
        var actionResult = fileAction(inputPath);
        if (actionResult.success)
        {
            result = actionResult.result;
            return true;
        }

        var newInput = inputPath;

        // Only PG knows why they only search for backslash and not also forward slash,
        // when in fact in other methods, they handle both. 
        var separatorIndex = inputPath.LastIndexOf('\\');
        if (separatorIndex != -1 && separatorIndex + 1 < inputPath.Length)
            newInput = inputPath.Substring(separatorIndex + 1);

        var pathWithFolders = FileSystem.Path.Combine("DATA\\ART\\TEXTURES", newInput);
        actionResult = fileAction(pathWithFolders);
        
        if (actionResult.success)
        {
            result = actionResult.result;
            return true;
        }
        
        return false;
    }


    private static string ChangeExtensionTo(string input, string extension)
    {
        // We cannot use Path.ChangeExtension as the PG implementation supports some strange things
        // like that a string "c:\\file.tga\\" ending with a directory separator. The PG result will be 
        // "c:\\file.dds" while Path.ChangeExtension would return "c:\\file.tga\\.dds"

        // Also, while there are many cases, where this method breaks (such as "c:/test.abc/path.dds"),
        // it's the way how the engine works 🤷‍
        var firstPart = input.Split('.')[0];
        return firstPart + extension;
    }
}