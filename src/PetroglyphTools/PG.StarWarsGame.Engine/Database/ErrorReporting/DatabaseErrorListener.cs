namespace PG.StarWarsGame.Engine.Database.ErrorReporting;

public abstract class DatabaseErrorListener : IDatabaseErrorListener
{
    public virtual void OnXmlError(XmlError error)
    {
    }

    public virtual void OnInitializationError(InitializationError error)
    {
    }
}