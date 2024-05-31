namespace PG.StarWarsGame.Engine.FileSystem;

public interface IGameRepositoryFactory
{
    IGameRepository Create(GameEngineType engineType, GameLocations gameLocations);
}