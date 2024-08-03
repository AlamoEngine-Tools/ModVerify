using System;
using System.Globalization;
using System.IO.Abstractions;
using System.Linq;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using ModVerify.CliApp.Options;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Infrastructure;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Name;

namespace ModVerify.CliApp.ModSelectors;

internal class SettingsBasedModSelector(IServiceProvider serviceProvider)
{
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
    public VerifyGameInstallationData CreateInstallationDataFromSettings(GameInstallationsSettings settings)
    {
        var gameLocations = new ModSelectorFactory(serviceProvider).CreateSelector(settings)
            .Select(settings, out var targetObject, out var engineType);

        if (gameLocations is null)
            throw new GameNotFoundException("Unable to get game locations");

        if (engineType is null)
            throw new InvalidOperationException("Engine type not specified.");

        return new VerifyGameInstallationData
        {
            EngineType = engineType.Value,
            GameLocations = gameLocations,
            Name = GetNameFromGameLocations(targetObject, gameLocations, engineType.Value)
        };
    }

    private string GetNameFromGameLocations(IPlayableObject? targetObject, GameLocations gameLocations, GameEngineType engineType)
    {
        if (targetObject is not null)
            return targetObject.Name;

        var mod = gameLocations.ModPaths.FirstOrDefault();

        var name = mod is not null ? GetNameFromMod(mod) : GetNameFromGame(engineType);

        if (string.IsNullOrEmpty(name))
            throw new InvalidOperationException("Mod or game name cannot be null or empty.");

        return name;
    }

    private string? GetNameFromGame(GameEngineType type)
    {
        var nameResolver = serviceProvider.GetRequiredService<IGameNameResolver>();
        return nameResolver.ResolveName(new GameIdentity(type.FromEngineType(), GamePlatform.Undefined), CultureInfo.InvariantCulture);
    }

    private string? GetNameFromMod(string mod)
    {
        var nameResolver = serviceProvider.GetRequiredService<IModNameResolver>();
        return nameResolver.ResolveName(new ModReference(_fileSystem.Path.GetFullPath(mod), ModType.Default), CultureInfo.InvariantCulture);
    }
}