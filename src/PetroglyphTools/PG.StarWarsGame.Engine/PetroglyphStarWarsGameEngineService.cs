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
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Engine.Rendering;
using PG.StarWarsGame.Engine.Rendering.Font;

namespace PG.StarWarsGame.Engine;

internal sealed class PetroglyphStarWarsGameEngineService(IServiceProvider serviceProvider)
    : IPetroglyphStarWarsGameEngineService
{
    private readonly IServiceProvider _serviceProvider =
        serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    private readonly ILogger? _logger = serviceProvider.GetService<ILoggerFactory>()
        ?.CreateLogger(typeof(PetroglyphStarWarsGameEngineService));

    public async Task<IStarWarsGameEngine> InitializeAsync(
        GameEngineType engineType,
        GameLocations gameLocations,
        IGameEngineErrorReporter? errorReporter = null,
        IProgress<string>? progress = null,
        bool cancelOnInitializationError = false,
        CancellationToken cancellationToken = default)

    {
        var errorListenerWrapper = new GameEngineErrorReporterWrapper(errorReporter);
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        errorListenerWrapper.InitializationError += OnInitializationError;

        try
        {
            return await InitializeEngine(engineType, gameLocations, errorListenerWrapper, progress, cts.Token)
                .ConfigureAwait(false);
        }
        finally
        {
            errorListenerWrapper.InitializationError -= OnInitializationError;
            errorListenerWrapper.Dispose();
            cts.Dispose();
        }

        void OnInitializationError(object sender, InitializationError e)
        {
            if (cancelOnInitializationError)
                cts.Cancel();
        }
    }

    private async Task<IStarWarsGameEngine> InitializeEngine(
        GameEngineType engineType,
        GameLocations gameLocations,
        GameEngineErrorReporterWrapper errorReporter,
        IProgress<string>? progress,
        CancellationToken token)
    {
        try
        {
            _logger?.LogInformation($"Initializing game engine for type '{engineType}'.");

            var repoFactory = _serviceProvider.GetRequiredService<IGameRepositoryFactory>();
            var repository = repoFactory.Create(engineType, gameLocations, errorReporter);

            var pgRender = new PGRender(repository, errorReporter, serviceProvider);

            var gameConstants = new GameConstants.GameConstants(repository, errorReporter, serviceProvider);
            progress?.Report("Initializing GameConstants");
            await gameConstants.InitializeAsync(token);

            // AudioConstants

            // MousePointer

            var fontManger = new FontManager(repository, errorReporter, serviceProvider);
            progress?.Report("Initializing FontManager");
            await fontManger.InitializeAsync(token);

            var guiDialogs = new GuiDialogGameManager(repository, errorReporter, serviceProvider);
            progress?.Report("Initializing GUIDialogManager");
            await guiDialogs.InitializeAsync(token);

            var sfxGameManager = new SfxEventGameManager(repository, errorReporter, serviceProvider);
            progress?.Report("Initializing SFXManager");
            await sfxGameManager.InitializeAsync(token);

            var commandBarManager = new CommandBarGameManager(repository, pgRender, gameConstants, fontManger, errorReporter, serviceProvider);
            progress?.Report("Initializing CommandBar");
            await commandBarManager.InitializeAsync(token);

            var gameObjetTypeManager = new GameObjectTypeGameManager(repository, errorReporter, serviceProvider);
            progress?.Report("Initializing GameObjectTypeManager");
            await gameObjetTypeManager.InitializeAsync(token);

            token.ThrowIfCancellationRequested();

            repository.Seal();

            return new GameEngine
            {
                EngineType = engineType,
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
            _logger?.LogDebug("Finished initializing game database.");
        }
    }
}