using System.Threading;
using System.Threading.Tasks;

namespace PG.StarWarsGame.Engine.Database;

public interface IGameDatabaseService
{
    Task<IGameDatabase> CreateDatabaseAsync(
        GameEngineType targetEngineType, 
        GameLocations locations,
        CancellationToken cancellationToken = default);
}