using System;
using AnakinRaW.ApplicationBase;

namespace AET.ModVerify.App.Utilities;

internal static class ModVerifyConsoleUtilities
{
    public static void WriteHeader(string? version = null)
    {
        const int lineLength = 73;
        const string author = "by AnakinRaW";

        ConsoleUtilities.WriteHorizontalLine('*', lineLength);
        Console.WriteLine(Figgle.FiggleFonts.Standard.Render(ModVerifyConstants.AppNameString));
        if (!string.IsNullOrEmpty(version))
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            ConsoleUtilities.WriteLineRight($"Version: {version}", lineLength);
            Console.ResetColor();
            Console.WriteLine();
        }
        ConsoleUtilities.WriteHorizontalLine('*', lineLength);

        ConsoleUtilities.WriteLineRight(author, lineLength);
        Console.WriteLine();
        Console.WriteLine();
    }
}