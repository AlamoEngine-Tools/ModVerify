using System.Threading;
using System.Threading.Tasks;
using PG.StarWarsGame.Engine.Database.ErrorReporting;

namespace PG.StarWarsGame.Engine.Database;

public interface IGameDatabaseService
{
    Task<IGameDatabase> InitializeGameAsync(
        GameEngineType targetEngineType, 
        GameLocations locations,
        IDatabaseErrorListener? errorListener = null,
        CancellationToken cancellationToken = default);
}