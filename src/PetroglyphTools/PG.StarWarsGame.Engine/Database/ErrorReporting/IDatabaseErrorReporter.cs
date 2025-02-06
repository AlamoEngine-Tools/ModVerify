namespace PG.StarWarsGame.Engine.Database.ErrorReporting;

public interface IDatabaseErrorReporter
{
    void Report(XmlError error);

    void Report(InitializationError error);
}