using System;
using System.Threading;
using AnakinRaW.CommonUtilities.SimplePipeline.Steps;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine.Repositories;

namespace PG.StarWarsGame.Engine.Database.Initialization;

internal abstract class CreateDatabaseStep<T>(IGameRepository repository, IServiceProvider serviceProvider)
    : PipelineStep(serviceProvider) where T : class
{
    public T Database { get; private set; } = null!;

    protected abstract string Name { get; }

    protected IGameRepository GameRepository { get; } = repository;

    protected sealed override void RunCore(CancellationToken token)
    {
        Logger?.LogDebug($"Creating {Name} database...");
        Database = CreateDatabase();
    }

    protected abstract T CreateDatabase();
}