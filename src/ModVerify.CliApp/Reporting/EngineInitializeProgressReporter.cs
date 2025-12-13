using System;
using PG.StarWarsGame.Engine;

namespace AET.ModVerify.App.Reporting;

internal sealed class EngineInitializeProgressReporter(GameEngineType engine) : IGameEngineInitializationReporter
{ 
    public void ReportProgress(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public void ReportStarted()
    {
        Console.WriteLine($"Initializing game engine '{engine}'...");
    }

    public void ReportFinished()
    {
        Console.WriteLine($"Game engine initialized.");
        Console.WriteLine();
    }
}