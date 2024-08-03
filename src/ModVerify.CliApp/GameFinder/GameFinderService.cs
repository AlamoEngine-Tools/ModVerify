using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Infrastructure.Clients.Steam;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using PG.StarWarsGame.Infrastructure.Services.Detection;

namespace AET.ModVerifyTool.GameFinder;

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

    public GameFinderResult FindGames()
    {
        var detectors = new List<IGameDetector>
        {
            new DirectoryGameDetector(_fileSystem.DirectoryInfo.New(Environment.CurrentDirectory), _serviceProvider),
            new SteamPetroglyphStarWarsGameDetector(_serviceProvider),
        };

        return FindGames(detectors);
    }

    public GameFinderResult FindGamesFromPathOrGlobal(string path)
    {
        // There are four common situations: 
        // 1. path points to the actual game directory
        // 2. path points to a local mod in game/Mods/ModDir
        // 3. path points to a workshop mod
        // 4. path points to a "detached mod" at a completely different location
        var givenDirectory = _fileSystem.DirectoryInfo.New(path);
        var possibleGameDir = givenDirectory.Parent?.Parent;
        
        var detectors = new List<IGameDetector>
        {
            // Case 1
            new DirectoryGameDetector(givenDirectory, _serviceProvider)
        };

        // Case 2
        if (possibleGameDir is not null)
            detectors.Add(new DirectoryGameDetector(possibleGameDir, _serviceProvider));

        // Cases 3 & 4
        detectors.Add(new SteamPetroglyphStarWarsGameDetector(_serviceProvider));
        return FindGames(detectors);
    }

    private bool TryDetectGame(GameType gameType, IList<IGameDetector> detectors, out GameDetectionResult result)
    {
        var gd = new CompositeGameDetector(detectors, _serviceProvider);
        result = gd.Detect(new GameDetectorOptions(gameType));

        if (result.Error is not null)
        {
            _logger?.LogTrace($"Unable to find game installation: {result.Error.Message}", result.Error);
            return false;
        }
        if (result.GameLocation is null)
            return false;

        return true;
    }

    private GameFinderResult FindGames(IList<IGameDetector> detectors)
    {
        // FoC needs to be tried first
        if (!TryDetectGame(GameType.Foc, detectors, out var result))
        {
            _logger?.LogTrace("Unable to find FoC installation. Trying again with EaW...");
            if (!TryDetectGame(GameType.Eaw, detectors, out result))
                throw new GameNotFoundException("Unable to find game installation: Wrong install path?");
        }

        if (result.GameLocation is null)
            throw new GameNotFoundException("Unable to find game installation: Wrong install path?");

        _logger?.LogTrace($"Found game installation: {result.GameIdentity} at {result.GameLocation.FullName}");

        var game = _gameFactory.CreateGame(result, CultureInfo.InvariantCulture);

        SetupMods(game);


        IGame? fallbackGame = null;
        // If the game is Foc we want to set up Eaw as well as the fallbackGame
        if (game.Type == GameType.Foc)
        {
            var fallbackDetectors = new List<IGameDetector>();
            
            if (game.Platform == GamePlatform.SteamGold)
                fallbackDetectors.Add(new SteamPetroglyphStarWarsGameDetector(_serviceProvider));
            else
                throw new NotImplementedException("Searching fallback game for non-Steam games is currently is not yet implemented.");

            if (!TryDetectGame(GameType.Eaw, fallbackDetectors, out var fallbackResult) || fallbackResult.GameLocation is null)
                throw new GameNotFoundException("Unable to find fallback game installation: Wrong install path?");

            _logger?.LogTrace($"Found fallback game installation: {fallbackResult.GameIdentity} at {fallbackResult.GameLocation.FullName}");

            fallbackGame = _gameFactory.CreateGame(fallbackResult, CultureInfo.InvariantCulture);

            SetupMods(fallbackGame);
        }

        return new GameFinderResult(game, fallbackGame);
    }

    private void SetupMods(IGame game)
    {
        var modFinder = _serviceProvider.GetRequiredService<IModReferenceFinder>();
        var modRefs = modFinder.FindMods(game);

        var mods = new List<IMod>();

        foreach (var modReference in modRefs)
        {
            var mod = _modFactory.FromReference(game, modReference, CultureInfo.InvariantCulture);
            mods.AddRange(mod);
        }

        foreach (var mod in mods)
            game.AddMod(mod);

        // Mods need to be added to the game first, before resolving their dependencies.
        foreach (var mod in mods)
        {
            var resolver = _serviceProvider.GetRequiredService<IDependencyResolver>();
            mod.ResolveDependencies(resolver,
                new DependencyResolverOptions { CheckForCycle = true, ResolveCompleteChain = true });
        }
    }
}