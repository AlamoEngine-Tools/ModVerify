namespace PG.StarWarsGame.Engine.Localization;

public interface IGameLanguageManagerProvider
{
    IGameLanguageManager GetLanguageManager(GameEngineType engine);
}