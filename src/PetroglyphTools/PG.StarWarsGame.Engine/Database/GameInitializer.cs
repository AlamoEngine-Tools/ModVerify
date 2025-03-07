using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine.Audio.Sfx;
using PG.StarWarsGame.Engine.CommandBar;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.GameObjects;
using PG.StarWarsGame.Engine.GuiDialog;
using PG.StarWarsGame.Engine.IO.Repositories;
using PG.StarWarsGame.Engine.Rendering;
using PG.StarWarsGame.Engine.Rendering.Font;

namespace PG.StarWarsGame.Engine.Database;

internal class GameInitializer(GameRepository repository, bool cancelOnError, IServiceProvider serviceProvider)
{
    private readonly ILogger? _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(GameInitializer));

    private CancellationTokenSource? _cancellationTokenSource;

    public async Task<IGameDatabase> InitializeAsync(GameErrorReporterWrapper errorReporter, CancellationToken token)
    {
        _logger?.LogInformation("Initializing Game Database...");
        errorReporter.InitializationError += OnInitializationError;

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
            // TradeRouteFiles.xml
            // HardPointDataFiles.xml
            // CampaignFiles.xml
            // FactionFiles.xml
            // TargetingPrioritySetFiles.xml
            // MousePointerFiles.xml

            var pgRender = new PGRender(repository, errorReporter, serviceProvider);
            
            var gameConstants = new GameConstants.GameConstants(repository, errorReporter, serviceProvider);
            await gameConstants.InitializeAsync( _cancellationTokenSource.Token);

            // AudioConstants

            // MousePointer

            var fontManger = new FontManager(repository, errorReporter, serviceProvider);
            await fontManger.InitializeAsync(_cancellationTokenSource.Token);

            var guiDialogs = new GuiDialogGameManager(repository, errorReporter, serviceProvider);
            await guiDialogs.InitializeAsync(_cancellationTokenSource.Token);

            var sfxGameManager = new SfxEventGameManager(repository, errorReporter, serviceProvider);
            await sfxGameManager.InitializeAsync( _cancellationTokenSource.Token);

            var commandBarManager = new CommandBarGameManager(repository, pgRender, gameConstants, fontManger, errorReporter, serviceProvider);
            await commandBarManager.InitializeAsync( _cancellationTokenSource.Token);

            var gameObjetTypeManager = new GameObjectTypeGameManager(repository, errorReporter, serviceProvider);
            await gameObjetTypeManager.InitializeAsync( _cancellationTokenSource.Token);

            repository.Seal();

            return new GameDatabase
            {
                PGRender = pgRender,
                FontManager = fontManger,
                GameRepository = repository,
                GameConstants = gameConstants,
                GuiDialogManager = guiDialogs,
                CommandBar = commandBarManager,
                GameObjectTypeManager = gameObjetTypeManager,
                SfxGameManager = sfxGameManager,
                InstalledLanguages = sfxGameManager.InstalledLanguages
            };
        }
        finally
        {
            errorReporter.InitializationError -= OnInitializationError;
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