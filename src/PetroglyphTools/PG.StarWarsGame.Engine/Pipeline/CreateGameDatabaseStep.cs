using System;
using System.Threading;
using AnakinRaW.CommonUtilities.SimplePipeline.Steps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine.FileSystem;

namespace PG.StarWarsGame.Engine.Pipeline;

public class CreateGameDatabaseStep : SynchronizedStep
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger? _logger;

    public GameDatabase GameDatabase { get; private set; } = null!;

    public CreateGameDatabaseStep(IGameRepository gameRepository, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _logger = Services.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        _gameRepository = gameRepository;
    }

    protected override void RunSynchronized(CancellationToken token)
    {
        _logger?.LogInformation("Creating Game Database...");
        var indexGamesPipeline = new CreateGameDatabasePipeline(_gameRepository, Services);
        indexGamesPipeline.RunAsync(token).Wait();
        GameDatabase = indexGamesPipeline.GameDatabase;
        _logger?.LogInformation("Finished creating game database");
    }
}