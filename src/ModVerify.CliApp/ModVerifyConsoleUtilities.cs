using System;
using AnakinRaW.ApplicationBase;

namespace AET.ModVerifyTool;

internal static class ModVerifyConsoleUtilities
{
    public static void WriteHeader()
    {
        const int lineLength = 50;
        const string author = "by AnakinRaW";

        ConsoleUtilities.WriteHorizontalLine('*', lineLength);

        Console.WriteLine(Figgle.FiggleFonts.Standard.Render(ModVerifyConstants.AppNameString));
        ConsoleUtilities.WriteHorizontalLine('*', lineLength);

        Console.WriteLine(new string(' ', lineLength - author.Length)+ author);
        Console.WriteLine();
        Console.WriteLine();
    }
}