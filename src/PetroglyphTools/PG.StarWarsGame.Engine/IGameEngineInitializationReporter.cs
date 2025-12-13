namespace PG.StarWarsGame.Engine;

public interface IGameEngineInitializationReporter
{
    void ReportProgress(string message);

    void ReportStarted();

    void ReportFinished();
}