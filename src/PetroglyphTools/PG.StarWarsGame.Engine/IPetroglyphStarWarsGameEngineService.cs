using System.Threading;
using System.Threading.Tasks;
using PG.StarWarsGame.Engine.ErrorReporting;

namespace PG.StarWarsGame.Engine;

public interface IPetroglyphStarWarsGameEngineService
{
    public Task<IStarWarsGameEngine> InitializeAsync(
        GameEngineType engineType,
        GameLocations gameLocations,
        IGameEngineErrorReporter? errorReporter = null,
        IGameEngineInitializationReporter? initReporter = null, 
        bool cancelOnInitializationError = false,
        CancellationToken cancellationToken = default);
}