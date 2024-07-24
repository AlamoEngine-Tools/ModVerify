namespace PG.StarWarsGame.Engine.Language;

public interface IGameLanguageManagerProvider
{
    IGameLanguageManager GetLanguageManager(GameEngineType engine);
}