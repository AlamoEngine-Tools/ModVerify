using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine.Database.ErrorReporting;
using PG.StarWarsGame.Engine.GameManagers;
using PG.StarWarsGame.Engine.Repositories;

namespace PG.StarWarsGame.Engine.Database;

internal class GameInitializer(
    GameRepository repository,
    IServiceProvider serviceProvider)
{
    private readonly ILogger? _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(GameInitializer));

    public async Task<IGameDatabase> InitializeAsync(DatabaseErrorListenerWrapper errorListener, CancellationToken token)
    {
        _logger?.LogInformation("Initializing Game Database...");

        try
        { 
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

            var gameConstants = new GameConstants(repository, serviceProvider);
            await gameConstants.InitializeAsync(errorListener, token);

            var sfxGameManager = new SfxEventGameManager(repository, serviceProvider);
            await sfxGameManager.InitializeAsync(errorListener, token);

            var gameObjetManager = new GameObjectGameManager(repository, serviceProvider);
            await gameObjetManager.InitializeAsync(errorListener, token);

            repository.Seal();

            return new GameDatabase
            {
                GameRepository = repository,
                GameConstants = gameConstants,//_parseGameConstants.Database,
                GameObjectManager = gameObjetManager,
                SfxGameManager = sfxGameManager,
                InstalledLanguages = sfxGameManager.InstalledLanguages
            };
        }
        finally
        {
            _logger?.LogInformation("Finished initializing game database");
        }
    }
}