namespace PG.StarWarsGame.Engine.ErrorReporting;

public abstract class GameErrorReporter : IGameErrorReporter
{
    public virtual void Report(XmlError error)
    {
    }

    public virtual void Report(InitializationError error)
    {
    }

    public virtual void Assert(EngineAssert assert)
    {
    }
}