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
using PG.StarWarsGame.Engine.GameObjects;

namespace PG.StarWarsGame.Engine.CommandBar;

internal partial class CommandBarGameManager(
    GameRepository repository,
    PGRender pgRender,
    IGameConstants gameConstants,
    IFontManager fontManager,
    GameEngineErrorReporterWrapper errorReporter,
    IServiceProvider serviceProvider)
    : GameManagerBase<CommandBarBaseComponent>(repository, errorReporter, serviceProvider), ICommandBarGameManager
{
    private readonly ICrc32HashingService _hashingService = serviceProvider.GetRequiredService<ICrc32HashingService>();
    private readonly IMtdFileService _mtdFileService = serviceProvider.GetRequiredService<IMtdFileService>();
    private readonly Dictionary<string, CommandBarComponentGroup> _groups = new();

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

    public IMtdFile? MtdFile
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

    public string? MegaTextureFileName
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

    public bool IconExists(GameObjectType gameObjectType)
    {
        ThrowIfNotInitialized();
        if (gameObjectType == null)
            throw new ArgumentNullException(nameof(gameObjectType));

        if (MtdFile is null)
            return false;

        if (string.IsNullOrEmpty(gameObjectType.IconName))
            return false;

        var crc = _hashingService.GetCrc32Upper(gameObjectType.IconName, PGConstants.DefaultPGEncoding);

        return MtdFile.Content.Contains(crc);
    }
}