using AnakinRaW.ApplicationBase;
using Figgle;
using System;
using System.Collections.Generic;
using AET.ModVerify.Reporting.Baseline;

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

    public static void WriteSelectedTarget(VerificationTarget target)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Selected Target:");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        ConsoleUtilities.PrintAsTable([
            ("Name", target.Name),
            ("Type", target.IsGame ? "Game" : "Mod"),
            ("Engine", target.Engine),
            ("Version", target.Version ?? "n/a"),
            ("Location", target.Location.TargetPath),
        ], 120);
        Console.ResetColor();
    }

    public static void WriteBaselineInfo(VerificationBaseline baseline, string? filePath)
    {
        if (baseline.IsEmpty)
            return;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Using Baseline:");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        
        IList<(string, object)> baselineData =
        [
            ("Version", baseline.Version?.ToString(2) ?? "n/a"),
            ("Is Default", filePath is null),
            ("Minimum Severity", baseline.MinimumSeverity.ToString()),
            ("Entries", baseline.Count.ToString())
        ];
        if (!string.IsNullOrEmpty(filePath))
            baselineData.Add(("File Path", filePath));

        ConsoleUtilities.PrintAsTable(baselineData, 120);

        if (baseline.Target is not null)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("Baseline Target:");
            Console.ForegroundColor = ConsoleColor.DarkGray;

            IList<(string, object)> targetData = [
                ("Name", baseline.Target.Name),
                ("Type", baseline.Target.IsGame ? "Game" : "Mod"),
                ("Engine", baseline.Target.Engine),
                ("Version", baseline.Target.Version ?? "n/a"),
            ];

            if (baseline.Target.Location is not null)
                targetData.Add(("Location", baseline.Target.Location.TargetPath));

            ConsoleUtilities.PrintAsTable(targetData, 120);
        }
        Console.ResetColor();
    }
}