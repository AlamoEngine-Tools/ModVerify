using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using AET.ModVerify.App.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Infrastructure.Clients.Steam;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Games.Registry;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services;
using PG.StarWarsGame.Infrastructure.Services.Detection;

namespace AET.ModVerify.App.GameFinder;

internal sealed class GameFinderSettings
{
    internal static readonly GameFinderSettings Default = new();
    
    public bool InitMods { get; init; } = true;
    
    public bool SearchFallbackGame { get; init; } = true;

    public GameEngineType? Engine { get; init; } = null;
}

internal class GameFinderService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger? _logger;
    private readonly IGameFactory _gameFactory;
    private readonly IModFactory _modFactory;

    public GameFinderService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _fileSystem = _serviceProvider.GetRequiredService<IFileSystem>();
        _gameFactory = _serviceProvider.GetRequiredService<IGameFactory>();
        _modFactory = _serviceProvider.GetRequiredService<IModFactory>();
        _logger = _serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    public GameFinderResult FindGames(GameFinderSettings settings)
    {
        var detectors = new List<IGameDetector>
        {
            new DirectoryGameDetector(_fileSystem.DirectoryInfo.New(Environment.CurrentDirectory), _serviceProvider),
            new SteamPetroglyphStarWarsGameDetector(_serviceProvider),
        };

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var registryFactory = _serviceProvider.GetRequiredService<IGameRegistryFactory>();
            detectors.Add(new RegistryGameDetector(
                registryFactory.CreateRegistry(GameType.Eaw), 
                registryFactory.CreateRegistry(GameType.Foc), 
                false, _serviceProvider));
        }

        return FindGames(detectors, settings);
    }

    public IGame FindGame(string gamePath, GameFinderSettings settings)
    {
        var detectors = new List<IGameDetector>
        {
            new DirectoryGameDetector(_fileSystem.DirectoryInfo.New(gamePath), _serviceProvider),
        };
        return FindGames(detectors, settings).Game;
    }

    public bool TryFindGame(string gamePath, GameFinderSettings settings, [NotNullWhen(true)]out IGame? game)
    {
        var detectors = new List<IGameDetector>
        {
            new DirectoryGameDetector(_fileSystem.DirectoryInfo.New(gamePath), _serviceProvider),
        };

        try
        {
            game = FindGames(detectors, settings).Game;
            return true;
        }
        catch (GameNotFoundException)
        {
            game = null;
            return false;
        }
    }
    
    public GameFinderResult FindGamesFromPathOrGlobal(string path, GameFinderSettings settings)
    {
        // There are four common situations: 
        // 1. path points to the actual game directory
        // 2. path points to a local mod in game/Mods/ModDir
        // 3. path points to a workshop mod
        // 4. path points to a "detached mod" at a completely different location
        var givenDirectory = _fileSystem.DirectoryInfo.New(path);
        var possibleGameDir = givenDirectory.Parent?.Parent;


        // We need to check the local paths first, before falling back to global detectors,
        // to ensure that we always find the correct game installation,
        // especially if the user did not request a specific engine.
        
        var localDetectors = new List<IGameDetector>
        {
            new DirectoryGameDetector(givenDirectory, _serviceProvider),
        };
        if (possibleGameDir is not null) 
            localDetectors.Add(new DirectoryGameDetector(possibleGameDir, _serviceProvider));

        // Case 1 & 2
        if (TryFindGames(localDetectors, settings, out var finderResult))
            return finderResult;


        // There is the rare scenario where the user specified a specific engine, 
        // but path points to a game with the opposite engine.
        // This does not make sense and the global detectors can not handle this.
        // Thus, we need to check against this.
        if (settings.Engine is not null)
        {
            if (TryFindGame(path, new GameFinderSettings
                {
                    Engine = settings.Engine.Value.Opposite(),
                    InitMods = false,
                    SearchFallbackGame = false
                }, out _))
            {
                var e = new GameNotFoundException(
                    $"The specified game engine '{settings.Engine.Value}' does not match engine of the specified path '{path}'.");
                _logger?.LogTrace(e, e.Message);
                throw e;
            }
        }

        // Cases 3 & 4
        return FindGames(CreateGlobalDetectors(), settings);
    }

    private bool TryFindGames(IList<IGameDetector> detectors, GameFinderSettings settings, 
        [NotNullWhen(true)] out GameFinderResult? finderResult)
    {
        try
        {
            finderResult = FindGames(detectors, settings);
            return true;
        }
        catch (GameNotFoundException)
        {
            finderResult = null;
            return false;
        }
    }

    private GameFinderResult FindGames(IList<IGameDetector> detectors, GameFinderSettings settings)
    {
        GameDetectionResult? detectionResult = null;
        if (settings.Engine is GameEngineType.Eaw)
        {
            _logger?.LogTrace("Trying to find requested EaW installation.");
            if (!TryDetectGame(GameType.Eaw, detectors, out detectionResult))
            {
                var e = new GameNotFoundException($"Unable to find requested game installation '{settings.Engine}'. Wrong install path?");
                _logger?.LogTrace(e, e.Message);
                throw e;
            }
        }
        
        if (detectionResult is null && !TryDetectGame(GameType.Foc, detectors, out detectionResult))
        {
            if (settings.Engine is GameEngineType.Foc)
            {
                var e = new GameNotFoundException($"Unable to find requested game installation '{settings.Engine}'. Wrong install path?");
                _logger?.LogTrace(e, e.Message);
                throw e;
            }
            
            // If the engine is unspecified, we also need to check for EaW.
            _logger?.LogTrace("Unable to find FoC installation. Trying again with EaW...");
            if (!TryDetectGame(GameType.Eaw, detectors, out detectionResult))
                throw new GameNotFoundException("Unable to find game installation: Wrong install path?");
        }

        Debug.Assert(detectionResult.GameLocation is not null);

        _logger?.LogInformation(ModVerifyConstants.ConsoleEventId, 
            "Found game installation: {ResultGameIdentity} at {GameLocationFullName}",
            detectionResult.GameIdentity, detectionResult.GameLocation!.FullName);

        var game = _gameFactory.CreateGame(detectionResult, CultureInfo.InvariantCulture);

        if (settings.InitMods) 
            SetupMods(game);

        IGame? fallbackGame = null;
        if (SearchForFallbackGame(settings, detectionResult))
        {
            if (!TryDetectGame(GameType.Eaw, CreateGlobalDetectors(), out var fallbackResult))
                throw new GameNotFoundException("Unable to find fallback game installation: Wrong install path?");

            _logger?.LogInformation(ModVerifyConstants.ConsoleEventId,
                "Found fallback game installation: {FallbackResultGameIdentity} at {GameLocationFullName}", 
                fallbackResult.GameIdentity, fallbackResult.GameLocation!.FullName);

            fallbackGame = _gameFactory.CreateGame(fallbackResult, CultureInfo.InvariantCulture);

            if (settings.InitMods)
                SetupMods(fallbackGame);
        }

        return new GameFinderResult(game, fallbackGame);
    }

    private static bool SearchForFallbackGame(GameFinderSettings settings, GameDetectionResult? foundGame)
    {
        if (settings.Engine is GameEngineType.Eaw)
            return false;
        if (foundGame is { Installed: true, GameIdentity.Type: GameType.Eaw })
            return false;
        return settings.SearchFallbackGame;
    }

    private bool TryDetectGame(GameType gameType, IList<IGameDetector> detectors, out GameDetectionResult result)
    {
        var gd = new CompositeGameDetector(detectors, _serviceProvider, true);
        result = gd.Detect(gameType);
        return result.GameLocation is not null;
    }

    private void SetupMods(IGame game)
    {
        var modFinder = _serviceProvider.GetRequiredService<IModFinder>();
        var modRefs = modFinder.FindMods(game);

        var mods = new List<IMod>();

        foreach (var modReference in modRefs)
        {
            var mod = _modFactory.CreatePhysicalMod(game, modReference, CultureInfo.InvariantCulture);
            mods.Add(mod);
        }

        foreach (var mod in mods)
            game.AddMod(mod);

        // Mods need to be added to the game first, before resolving their dependencies.
        foreach (var mod in mods)
        {
            mod.ResolveDependencies();
        }
    }

    private IList<IGameDetector> CreateGlobalDetectors()
    {
        var detectors = new List<IGameDetector>
        {
            new SteamPetroglyphStarWarsGameDetector(_serviceProvider),
        };
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var registryFactory = _serviceProvider.GetRequiredService<IGameRegistryFactory>();
            detectors.Add(new RegistryGameDetector(
                registryFactory.CreateRegistry(GameType.Eaw),
                registryFactory.CreateRegistry(GameType.Foc),
                false, _serviceProvider));
        }
        return detectors;
    }
}