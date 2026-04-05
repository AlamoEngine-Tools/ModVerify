using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;

namespace PG.StarWarsGame.Engine.GameObjects;

internal partial class GameObjectTypeGameManager : GameManagerBase<GameObjectType>, IGameObjectTypeGameManager
{
    private readonly List<GameObjectType> _gameObjects;
    private readonly ICrc32HashingService _hashingService;

    public GameObjectTypeGameManager(
        GameRepository repository, 
        GameEngineErrorReporterWrapper errorReporter, 
        IServiceProvider serviceProvider)
        : base(repository, errorReporter, serviceProvider)
    {
        _hashingService = serviceProvider.GetRequiredService<ICrc32HashingService>();
        _gameObjects = [];
        GameObjectTypes = new ReadOnlyCollection<GameObjectType>(_gameObjects);
    }
    
    public IReadOnlyList<GameObjectType> GameObjectTypes
    {
        get
        {
            ThrowIfNotInitialized();
            return field;
        }
    }
    
    public GameObjectType? FindObjectType(string? name)
    {
        if (string.IsNullOrEmpty(name))
            return null;
        var nameCrc = _hashingService.GetCrc32Upper(name, PGConstants.DefaultPGEncoding);
        NamedEntries.TryGetFirstValue(nameCrc, out var gameObject);
        return gameObject;
    }
}