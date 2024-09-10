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

internal abstract class GameManagerBase<T>(GameRepository repository, DatabaseErrorListenerWrapper errorListener, IServiceProvider serviceProvider)
    : GameManagerBase(repository, errorListener, serviceProvider), IGameManager<T>
{
    protected readonly ValueListDictionary<Crc32, T> NamedEntries = new();

    public ICollection<T> Entries => NamedEntries.Values;

    public ICollection<Crc32> EntryKeys => NamedEntries.Keys;

    public ReadOnlyFrugalList<T> GetEntries(Crc32 key)
    {
        return NamedEntries.GetValues(key);
    }
}

internal abstract class GameManagerBase
{
    public event EventHandler Initialized;

    private bool _initialized;
    private protected readonly GameRepository GameRepository;
    protected readonly IServiceProvider ServiceProvider;
    protected readonly IFileSystem FileSystem;
    protected readonly ILogger? Logger;

    protected readonly DatabaseErrorListenerWrapper ErrorListener;

    public bool IsInitialized => _initialized;

    protected GameManagerBase(GameRepository repository, DatabaseErrorListenerWrapper errorListener, IServiceProvider serviceProvider)
    {
        GameRepository = repository ?? throw new ArgumentNullException(nameof(repository));
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        FileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        ErrorListener = errorListener ?? throw new ArgumentNullException(nameof(errorListener));
    }

    public async Task InitializeAsync(CancellationToken token)
    {
        ThrowIfAlreadyInitialized();
        token.ThrowIfCancellationRequested();
        try
        {
            await InitializeCoreAsync(token);
            _initialized = true;
        }
        catch (Exception e)
        {
            Logger?.LogError(e, $"Initialization of {this} failed: {e.Message}");
            throw;
        }
        OnInitialized();
    }

    public override string ToString()
    {
        return GetType().Name;
    }

    protected abstract Task InitializeCoreAsync(CancellationToken token);

    protected void ThrowIfAlreadyInitialized()
    {
        if (_initialized)
            throw new InvalidOperationException("Game manager is already initialized.");
    }

    protected void ThrowIfNotInitialized()
    {
        if (!_initialized)
            throw new InvalidOperationException("Game manager not initialized.");
    }

    private void OnInitialized()
    {
        Initialized?.Invoke(this, EventArgs.Empty);
    }
}