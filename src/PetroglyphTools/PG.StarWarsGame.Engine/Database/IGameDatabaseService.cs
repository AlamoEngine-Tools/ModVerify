using System.Threading;
using System.Threading.Tasks;

namespace PG.StarWarsGame.Engine.Database;

public interface IGameDatabaseService
{
    Task<IGameDatabase> InitializeGameAsync(GameInitializationOptions gameInitializationOptions, CancellationToken cancellationToken = default);
}