using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace PG.StarWarsGame.Engine.Testing;

/// <summary>Builds a temp-directory-backed <see cref="VirtualGameRepo"/> from raw file content.</summary>
public sealed class VirtualGameRepoBuilder
{
    private readonly IServiceProvider _services;
    private readonly IFileSystem _fs;
    private readonly string _tempRoot;
    private readonly string _gameRoot;
    private readonly List<string> _modPaths = [];
    private readonly List<string> _fallbackPaths = [];
    private string? _fallbackGamePath;

    /// <summary>Initializes a new instance of the <see cref="VirtualGameRepoBuilder"/> class.</summary>
    /// <param name="services">The service provider supplying the file system and file-format services used by the builder.</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public VirtualGameRepoBuilder(IServiceProvider services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _fs = services.GetRequiredService<IFileSystem>();
        _tempRoot = _fs.Path.Combine(_fs.Path.GetTempPath(),
            $"PG.StarWarsGame.Engine.Testing.{Guid.NewGuid():N}");
        _gameRoot = _fs.Path.Combine(_tempRoot, "game");
        _fs.Directory.CreateDirectory(_gameRoot);
    }

    /// <summary>Configures files under the base game directory.</summary>
    /// <param name="configure">The writer callback.</param>
    /// <exception cref="ArgumentNullException"><paramref name="configure"/> is <see langword="null"/>.</exception>
    public VirtualGameRepoBuilder WithGame(Action<IRepoOriginWriter> configure)
    {
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        configure(new RepoOriginWriter(_services, _gameRoot));
        return this;
    }

    /// <summary>Configures files under the primary fallback game directory.</summary>
    /// <param name="configure">The writer callback.</param>
    /// <exception cref="ArgumentNullException"><paramref name="configure"/> is <see langword="null"/>.</exception>
    public VirtualGameRepoBuilder WithFallbackGame(Action<IRepoOriginWriter> configure)
    {
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        if (_fallbackGamePath == null)
        {
            _fallbackGamePath = _fs.Path.Combine(_tempRoot, "fallback", "_primary");
            _fs.Directory.CreateDirectory(_fallbackGamePath);
        }
        configure(new RepoOriginWriter(_services, _fallbackGamePath));
        return this;
    }

    /// <summary>Configures files under a named additional fallback path.</summary>
    /// <param name="name">A unique name identifying this fallback.</param>
    /// <param name="configure">The writer callback.</param>
    /// <exception cref="ArgumentException"><paramref name="name"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="configure"/> is <see langword="null"/>.</exception>
    public VirtualGameRepoBuilder WithFallback(string name, Action<IRepoOriginWriter> configure)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Fallback name must be non-empty.", nameof(name));
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        var dir = _fs.Path.Combine(_tempRoot, "fallback", name);
        if (!_fallbackPaths.Contains(dir))
        {
            _fs.Directory.CreateDirectory(dir);
            _fallbackPaths.Add(dir);
        }
        configure(new RepoOriginWriter(_services, dir));
        return this;
    }

    /// <summary>Configures files under a named mod path.</summary>
    /// <remarks>Mods are independent path roots. Declaration order is preserved as the order in <see cref="GameLocations.ModPaths"/>.</remarks>
    /// <param name="name">A unique name identifying this mod.</param>
    /// <param name="configure">The writer callback.</param>
    /// <exception cref="ArgumentException"><paramref name="name"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="configure"/> is <see langword="null"/>.</exception>
    public VirtualGameRepoBuilder WithMod(string name, Action<IRepoOriginWriter> configure)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Mod name must be non-empty.", nameof(name));
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        var dir = _fs.Path.Combine(_tempRoot, "mods", name);
        if (!_modPaths.Contains(dir))
        {
            _fs.Directory.CreateDirectory(dir);
            _modPaths.Add(dir);
        }
        configure(new RepoOriginWriter(_services, dir));
        return this;
    }

    /// <summary>Builds the configured repository.</summary>
    public VirtualGameRepo Build()
    {
        var fallbacks = new List<string>();
        if (_fallbackGamePath != null)
            fallbacks.Add(_fallbackGamePath);
        fallbacks.AddRange(_fallbackPaths);
        var locations = new GameLocations(_modPaths, _gameRoot, fallbacks);
        return new VirtualGameRepo(_fs, _tempRoot, locations);
    }
}
