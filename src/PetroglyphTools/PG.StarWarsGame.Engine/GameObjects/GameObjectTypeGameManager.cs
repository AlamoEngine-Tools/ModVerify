using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;

namespace PG.StarWarsGame.Engine.GameObjects;

internal partial class GameObjectTypeGameManager : GameManagerBase<GameObject>, IGameObjectTypeGameManager
{
    private readonly List<GameObject> _gameObjects;
    private readonly ICrc32HashingService _hashingService;

    public GameObjectTypeGameManager(
        GameRepository repository, 
        GameEngineErrorReporterWrapper errorReporter, 
        IServiceProvider serviceProvider)
        : base(repository, errorReporter, serviceProvider)
    {
        _hashingService = serviceProvider.GetRequiredService<ICrc32HashingService>();
        _gameObjects = [];
        GameObjects = new ReadOnlyCollection<GameObject>(_gameObjects);
    }


    public IReadOnlyList<GameObject> GameObjects
    {
        get
        {
            ThrowIfNotInitialized();
            return field;
        }
    }

    public IEnumerable<string> GetModels(GameObject gameObject)
    {
        var models = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddNotEmpty(gameObject.GalacticModel);
        AddNotEmpty(gameObject.DestroyedGalacticModel);
        AddNotEmpty(gameObject.LandModel);
        AddNotEmpty(gameObject.SpaceModel);
        AddNotEmpty(gameObject.GalacticFleetOverrideModel);
        AddNotEmpty(gameObject.GuiModel);
        AddNotEmpty(gameObject.ModelName);
        AddNotEmpty(gameObject.LandAnimOverrideModel);
        AddNotEmpty(gameObject.SpaceAnimOverrideModel);
        AddNotEmpty(gameObject.DamagedSmokeAssetModel);
        AddTerrainMappingModels();
        return models;


        void AddNotEmpty(string? value, Predicate<string>? predicate = null)
        {
            if (string.IsNullOrEmpty(value))
                return;
            if (predicate is null || predicate(value))
                models.Add(value);
        }


        void AddTerrainMappingModels()
        {
            var visitedEnvTypes = new HashSet<MapEnvironmentType>();
            foreach (var mapping in gameObject.LandTerrainModelMappingValues)
            {
                if (MapEnvironmentTypeConversion.ConversionDictionary.TryStringToEnum(mapping.terrain, out var envType))
                {
                    // The engine only uses the first model for each environment type.
                    if (visitedEnvTypes.Add(envType)) 
                        AddNotEmpty(mapping.model);
                }
            }
        }
    }

    public GameObject? FindObjectType(string? name)
    {
        if (string.IsNullOrEmpty(name))
            return null;
        var nameCrc = _hashingService.GetCrc32Upper(name, PGConstants.DefaultPGEncoding);
        NamedEntries.TryGetFirstValue(nameCrc, out var gameObject);
        return gameObject;
    }
}