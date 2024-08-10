using System.Threading;
using System.Threading.Tasks;

namespace PG.StarWarsGame.Engine.Database;

public interface IGameDatabaseService
{
    Task<IGameDatabase> InitializeGameAsync(
        GameEngineType targetEngineType, 
        GameLocations locations,
        IDatabaseErrorListener? errorListener = null,
        CancellationToken cancellationToken = default);
}