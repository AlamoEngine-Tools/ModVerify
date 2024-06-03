using System.Threading;
using System.Threading.Tasks;
using PG.StarWarsGame.Engine.Database;

namespace PG.StarWarsGame.Engine.Pipeline;

public interface IGameDatabaseService
{
    Task<IGameDatabase> CreateDatabaseAsync(GameEngineType targetEngineType, GameLocations locations, CancellationToken cancellationToken = default);
}