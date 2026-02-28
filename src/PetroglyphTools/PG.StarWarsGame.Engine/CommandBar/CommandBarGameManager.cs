using Microsoft.Extensions.DependencyInjection;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.CommandBar.Components;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.GameConstants;
using PG.StarWarsGame.Engine.IO.Repositories;
using PG.StarWarsGame.Engine.Rendering;
using PG.StarWarsGame.Engine.Rendering.Font;
using PG.StarWarsGame.Files.MTD.Files;
using PG.StarWarsGame.Files.MTD.Services;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace PG.StarWarsGame.Engine.CommandBar;

internal partial class CommandBarGameManager(
    GameEngineType engineType,
    GameRepository repository,
    PGRender pgRender,
    IGameConstants gameConstants,
    IFontManager fontManager,
    GameEngineErrorReporterWrapper errorReporter,
    IServiceProvider serviceProvider)
    : GameManagerBase<CommandBarBaseComponent>(engineType, repository, errorReporter, serviceProvider), ICommandBarGameManager
{
    private readonly ICrc32HashingService _hashingService = serviceProvider.GetRequiredService<ICrc32HashingService>();
    private readonly IMtdFileService _mtdFileService = serviceProvider.GetRequiredService<IMtdFileService>();
    private readonly Dictionary<string, CommandBarComponentGroup> _groups = new();

    private bool _megaTextureExists;
    private FontData? _defaultFont;

    public ICollection<CommandBarBaseComponent> Components => Entries;

    public IReadOnlyDictionary<string, CommandBarComponentGroup> Groups => _groups;

    public FontData? DefaultFont
    {
        get
        {
            ThrowIfNotInitialized();
            return _defaultFont;
        }
        private set
        {
            ThrowIfAlreadyInitialized();
            _defaultFont = value;
        }
    }

    public IMtdFile? MegaTextureFile
    {
        get
        {
            ThrowIfNotInitialized();
            return field;
        }
        private set
        {
            ThrowIfAlreadyInitialized();
            field = value;
        }
    }

    public Vector3 CommandBarScale { get; }

    public Vector3 CommandBarOffset { get; internal set; }
}