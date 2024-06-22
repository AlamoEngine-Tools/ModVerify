using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.CommonUtilities.SimplePipeline;
using AnakinRaW.CommonUtilities.SimplePipeline.Runners;
using AnakinRaW.CommonUtilities.SimplePipeline.Steps;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Engine.Repositories;

namespace PG.StarWarsGame.Engine.Database.Initialization;

internal class GameDatabaseCreationPipeline(GameRepository repository, IServiceProvider serviceProvider)
    : Pipeline(serviceProvider)
{
    private ParseSingletonXmlStep<GameConstants> _parseGameConstants = null!;
    private ParseXmlDatabaseFromContainerStep<GameObject> _parseGameObjects = null!;
    private ParseXmlDatabaseFromContainerStep<SfxEvent> _parseSfxEvents = null!;

    private ParallelRunner _parseXmlRunner = null!;

    public GameDatabase GameDatabase { get; private set; } = null!;


    protected override Task<bool> PrepareCoreAsync()
    {
        _parseXmlRunner = new ParallelRunner(4, ServiceProvider);
        foreach (var xmlParserStep in CreateXmlParserSteps())
            _parseXmlRunner.AddStep(xmlParserStep);


        return Task.FromResult(true);
    }

    private IEnumerable<PipelineStep> CreateXmlParserSteps()
    {
        yield return _parseGameConstants = new ParseSingletonXmlStep<GameConstants>("GameConstants",
            "DATA\\XML\\GAMECONSTANTS.XML", repository, ServiceProvider);

        yield return _parseGameObjects = new ParseXmlDatabaseFromContainerStep<GameObject>("GameObjects",
            "DATA\\XML\\GAMEOBJECTFILES.XML", repository, ServiceProvider);

        yield return _parseSfxEvents = new ParseXmlDatabaseFromContainerStep<SfxEvent>("SFXEvents",
            "DATA\\XML\\SFXEventFiles.XML", repository, ServiceProvider);

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
        // CommandBarComponentFiles.xml
        // TradeRouteFiles.xml
        // HardPointDataFiles.xml
        // CampaignFiles.xml
        // FactionFiles.xml
        // TargetingPrioritySetFiles.xml
        // MousePointerFiles.xml
    }

    protected override async Task RunCoreAsync(CancellationToken token)
    {
        Logger?.LogInformation("Creating Game Database...");

        try
        {
            try
            {
                Logger?.LogInformation("Parsing XML Files...");
                _parseXmlRunner.Error += OnError;
                await _parseXmlRunner.RunAsync(token);
            }
            finally
            {
                Logger?.LogInformation("Finished parsing XML Files...");
                _parseXmlRunner.Error -= OnError;
            }

            ThrowIfAnyStepsFailed(_parseXmlRunner.Steps);

            token.ThrowIfCancellationRequested();
            

            repository.Seal();

            GameDatabase = new GameDatabase
            {
                GameRepository = repository,
                GameConstants = _parseGameConstants.Database,
                GameObjects = _parseGameObjects.Database,
                SfxEvents = _parseSfxEvents.Database,
            };
        }
        finally
        {
            Logger?.LogInformation("Finished creating game database");
        }
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
}