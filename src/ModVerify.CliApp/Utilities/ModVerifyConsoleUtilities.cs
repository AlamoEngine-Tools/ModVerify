using AnakinRaW.ApplicationBase;
using Figgle;
using System;

namespace AET.ModVerify.App.Utilities;

[GenerateFiggleText("HeaderText", "standard", ModVerifyConstants.AppNameString)]
internal static partial class ModVerifyConsoleUtilities
{
    public static void WriteHeader(string? version = null)
    {
        const int lineLength = 73;
        const string author = "by AnakinRaW";

        ConsoleUtilities.WriteHorizontalLine('*', lineLength);
        Console.WriteLine(HeaderText);
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