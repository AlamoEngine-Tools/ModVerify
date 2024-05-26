using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.CommonUtilities.SimplePipeline;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Engine.FileSystem;

namespace PG.StarWarsGame.Engine.Pipeline;

internal class CreateGameDatabasePipeline(IGameRepository repository, IServiceProvider serviceProvider) 
    : ParallelPipeline(serviceProvider)
{
    private ParseSingletonXmlStep<GameConstants> _parseGameConstants = null!;
    private ParseFromContainerStep<GameObject> _parseGameObjects = null!;

    public GameDatabase GameDatabase { get; private set; } = null!;
    
    protected override Task<IList<IStep>> BuildSteps()
    {
        _parseGameConstants = new ParseSingletonXmlStep<GameConstants>("GameConstants", "DATA\\XML\\GAMECONSTANTS.XML", repository, ServiceProvider);
        _parseGameObjects = new ParseFromContainerStep<GameObject>("GameObjects", "DATA\\XML\\GAMEOBJECTFILES.XML", repository, ServiceProvider);

        // GUIDialogs.xml
        // LensFlares.xml
        // SurfaceFX.xml
        // TerrainDecalFX.xml
        // GraphicDetails.xml
        // DynamicTrackFX.xml
        // ShadowBlobMaterials.xml
        // TacticalCameras.xml
        // LightSources.xml
        // StarWars3DTextCrawl.xml
        // MusicEvents.xml
        // SpeechEvents.xml
        // GameConstants.xml
        // Audio.xml
        // WeatherAudio.xml
        // HeroClash.xml
        // TradeRouteLines.xml
        // RadarMap.xml
        // WeatherModifiers.xml
        // Movies.xml
        // LightningEffectTypes.xml
        // DifficultyAdjustments.xml
        // WeatherScenarios.xml
        // UnitAbilityTypes.xml
        // BlackMarketItems.xml
        // MovementClassTypeDefs.xml
        // AITerrainEffectiveness.xml


        // CONTAINER FILES:
        // GameObjectFiles.xml
        // SFXEventFiles.xml
        // CommandBarComponentFiles.xml
        // TradeRouteFiles.xml
        // HardPointDataFiles.xml
        // CampaignFiles.xml
        // FactionFiles.xml
        // TargetingPrioritySetFiles.xml
        // MousePointerFiles.xml

        return Task.FromResult<IList<IStep>>(new List<IStep>
        {
            _parseGameConstants,
            _parseGameObjects
        });
    }

    protected override async Task RunCoreAsync(CancellationToken token)
    {
        await base.RunCoreAsync(token);

        GameDatabase = new GameDatabase
        {
            GameConstants = _parseGameConstants.Database,
            GameObjects = _parseGameObjects.Database
        };
    }

    private sealed class ParseSingletonXmlStep<T>(string name, string xmlFile, IGameRepository repository, IServiceProvider serviceProvider)
        : ParseXmlDatabaseStep<T>(xmlFile, repository, serviceProvider) where T : class
    {
        protected override string Name => name;

        protected override T CreateDatabase(IList<T> parsedDatabaseEntries)
        {
            if (parsedDatabaseEntries.Count != 1)
                throw new InvalidOperationException($"There can be only one {Name} model.");

            return parsedDatabaseEntries.First();
        }
    }


    private sealed class ParseFromContainerStep<T>(
        string name,
        string xmlFile,
        IGameRepository repository,
        IServiceProvider serviceProvider) 
        : ParseXmlDatabaseFromContainerStep<IList<T>>(xmlFile, repository, serviceProvider)
    {
        protected override string Name => name;

        protected override IList<T> CreateDatabase(IList<IList<T>> parsedDatabaseEntries)
        {
            return parsedDatabaseEntries.SelectMany(x => x).ToList();
        }
    }
}