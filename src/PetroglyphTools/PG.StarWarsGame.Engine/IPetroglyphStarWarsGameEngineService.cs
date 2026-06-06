using System;
using System.Threading;
using System.Threading.Tasks;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO;

namespace PG.StarWarsGame.Engine;

public interface IPetroglyphStarWarsGameEngineService
{
    public Task<IStarWarsGameEngineHandle> InitializeAsync(
        GameEngineType engineType,
        GameLocations gameLocations,
        IGameEngineErrorReporter? errorReporter = null,
        IGameEngineInitializationReporter? initReporter = null,
        bool cancelOnInitializationError = false,
        Action<PetroglyphFileSystem>? configureFileSystem = null,
        CancellationToken cancellationToken = default);
}