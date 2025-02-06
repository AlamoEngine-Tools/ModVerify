namespace PG.StarWarsGame.Engine.Database.ErrorReporting;

public abstract class DatabaseErrorReporter : IDatabaseErrorReporter
{
    public virtual void Report(XmlError error)
    {
    }

    public virtual void Report(InitializationError error)
    {
    }
}