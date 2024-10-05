using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine.Database.ErrorReporting;
using PG.StarWarsGame.Engine.GameManagers;
using PG.StarWarsGame.Engine.IO.Repositories;

namespace PG.StarWarsGame.Engine.Database;

internal class GameInitializer(GameRepository repository, bool cancelOnError, IServiceProvider serviceProvider)
{
    private readonly ILogger? _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(GameInitializer));

    private CancellationTokenSource? _cancellationTokenSource;

    public async Task<IGameDatabase> InitializeAsync(DatabaseErrorListenerWrapper errorListener, CancellationToken token)
    {
        _logger?.LogInformation("Initializing Game Database...");
        errorListener.InitializationError += OnInitializationError;

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
        
        try
        { 
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
            // GameConstantsXml.xml
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
            
            var gameConstants = new GameConstants(repository, errorListener, serviceProvider);
            await gameConstants.InitializeAsync( _cancellationTokenSource.Token);

            var guiDialogs = new GuiDialogGameManager(repository, errorListener, serviceProvider);
            await guiDialogs.InitializeAsync(_cancellationTokenSource.Token);

            var sfxGameManager = new SfxEventGameManager(repository, errorListener, serviceProvider);
            await sfxGameManager.InitializeAsync( _cancellationTokenSource.Token);

            var commandBarManager = new CommandBarGameManager(repository, errorListener, serviceProvider);
            await commandBarManager.InitializeAsync( _cancellationTokenSource.Token);

            var gameObjetTypeManager = new GameObjectTypeTypeGameManager(repository, errorListener, serviceProvider);
            await gameObjetTypeManager.InitializeAsync( _cancellationTokenSource.Token);

            repository.Seal();

            return new GameDatabase
            {
                GameRepository = repository,
                GameConstants = gameConstants,
                GuiDialogManager = guiDialogs,
                CommandBarManager = commandBarManager,
                GameObjectTypeManager = gameObjetTypeManager,
                SfxGameManager = sfxGameManager,
                InstalledLanguages = sfxGameManager.InstalledLanguages
            };
        }
        finally
        {
            errorListener.InitializationError -= OnInitializationError;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            _logger?.LogInformation("Finished initializing game database");
        }
    }

    private void OnInitializationError(object sender, InitializationError e)
    {
        if(cancelOnError) 
            _cancellationTokenSource?.Cancel();
    }
}