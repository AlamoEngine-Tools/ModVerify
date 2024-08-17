using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.CommonUtilities.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.Database.ErrorReporting;
using PG.StarWarsGame.Engine.Repositories;
using PG.StarWarsGame.Files.XML;

namespace PG.StarWarsGame.Engine.GameManagers;

internal abstract class GameManagerBase
{
    private bool _initialized;
    private protected readonly GameRepository GameRepository;
    protected readonly IFileSystem FileSystem;
    protected readonly ILogger? Logger;

    protected GameManagerBase(GameRepository repository, IServiceProvider serviceProvider)
    {
        GameRepository = repository ?? throw new ArgumentNullException(nameof(repository));
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        FileSystem = serviceProvider.GetRequiredService<IFileSystem>();
    }

    public async Task InitializeAsync(DatabaseErrorListenerWrapper errorListener, CancellationToken token)
    {
        ThrowIfAlreadyInitialized();
        try
        {
            await InitializeCoreAsync(errorListener, token);
            _initialized = true;
        }
        catch (Exception e)
        {
            Logger?.LogError(e, $"Initialization of {this} failed: {e.Message}");
        }
    }
    protected abstract Task InitializeCoreAsync(DatabaseErrorListenerWrapper errorListener, CancellationToken token);

    protected void ThrowIfAlreadyInitialized()
    {
        if (_initialized)
            throw new InvalidOperationException("Game manager is already initialized");
    }
}

internal abstract class GameManagerBase<T>(GameRepository repository, IServiceProvider serviceProvider)
    : GameManagerBase(repository, serviceProvider), IGameManager<T>
{
    protected readonly ValueListDictionary<Crc32, T> NamedEntries = new();

    public ICollection<T> Entries => NamedEntries.Values;

    public ICollection<Crc32> EntryKeys => NamedEntries.Keys;

    public ReadOnlyFrugalList<T> GetEntries(Crc32 key)
    {
        return NamedEntries.GetValues(key);
    }
}