namespace PG.StarWarsGame.Engine.Database.ErrorReporting;

public interface IDatabaseErrorListener
{
    void OnXmlError(XmlError error);
    void OnInitializationError(InitializationError error);
}