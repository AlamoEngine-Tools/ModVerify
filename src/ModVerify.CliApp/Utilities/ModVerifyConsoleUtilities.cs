using AnakinRaW.ApplicationBase;
using Figgle;
using System;
using System.Collections.Generic;
using System.Linq;
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

    public static void WriteBaselineInfo(BaselineCollection baselines)
    {
        var displayable = baselines.Where(b => !b.Baseline.IsEmpty).ToList();
        if (displayable.Count == 0)
            return;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(displayable.Count == 1 ? "Using Baseline:" : "Using Baselines:");
        Console.ResetColor();

        for (var i = 0; i < displayable.Count; i++)
        {
            Console.WriteLine();
            WriteSingleBaseline(displayable[i], displayable.Count > 1 ? i + 1 : null);
        }
    }

    private static void WriteSingleBaseline(IdentifiedBaseline entry, int? index)
    {
        var baseline = entry.Baseline;
        var isDefault = entry.Source == BaselineSource.EmbeddedDefault;

        if (index is not null)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"[{index}]");
        }

        Console.ForegroundColor = ConsoleColor.DarkGray;
        ConsoleUtilities.PrintAsTable([
            ("Source", isDefault ? "Default (embedded)" : entry.Identifier),
            ("Version", baseline.Version?.ToString(2) ?? "n/a"),
            ("Minimum Severity", baseline.MinimumSeverity.ToString()),
            ("Entries", baseline.Count.ToString()),
        ], 120);

        if (baseline.Target is not null)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("Target:");
            Console.ForegroundColor = ConsoleColor.DarkGray;

            // Two-space prefix on each key indents the whole sub-table under "Target:".
            IList<(string, object)> targetData = [
                ("  Name", baseline.Target.Name),
                ("  Type", baseline.Target.IsGame ? "Game" : "Mod"),
                ("  Engine", baseline.Target.Engine),
                ("  Version", baseline.Target.Version ?? "n/a"),
            ];

            if (baseline.Target.Location is not null)
                targetData.Add(("  Location", baseline.Target.Location.TargetPath));

            ConsoleUtilities.PrintAsTable(targetData, 120);
        }
        Console.ResetColor();
    }
}