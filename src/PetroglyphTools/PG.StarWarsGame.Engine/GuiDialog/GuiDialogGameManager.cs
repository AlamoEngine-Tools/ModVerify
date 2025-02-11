using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Engine.Database.ErrorReporting;
using PG.StarWarsGame.Engine.GuiDialog.Xml;
using PG.StarWarsGame.Engine.IO.Repositories;
using PG.StarWarsGame.Files.MTD.Binary;
using PG.StarWarsGame.Files.MTD.Files;
using PG.StarWarsGame.Files.MTD.Services;

namespace PG.StarWarsGame.Engine.GuiDialog;

internal partial class GuiDialogGameManager(GameRepository repository, DatabaseErrorReporterWrapper errorReporter, IServiceProvider serviceProvider)
    : GameManagerBase(repository, errorReporter, serviceProvider), IGuiDialogManager
{
    private readonly IMtdFileService _mtdFileService = serviceProvider.GetRequiredService<IMtdFileService>();
    private readonly ICrc32HashingService _hashingService = serviceProvider.GetRequiredService<ICrc32HashingService>();

    // Unlike other strings for this game, the component's (aka gadget) name is case-sensitive. 
    private readonly Dictionary<string, Dictionary<GuiComponentType, ComponentTextureEntry>> _perComponentTextures = new(StringComparer.Ordinal);
    private readonly Dictionary<GuiComponentType, ComponentTextureEntry> _defaultTextures = new();
    private ReadOnlyDictionary<GuiComponentType, ComponentTextureEntry> _defaultTexturesRo = null!;

    private bool _megaTextureExists;
    private string? _megaTextureFileName;


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

    public GuiDialogsXml? GuiDialogsXml
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

    public IReadOnlyCollection<string> Components
    {
        get
        {
            ThrowIfNotInitialized();
            return _perComponentTextures.Keys;
        }
    }

    public IReadOnlyDictionary<GuiComponentType, ComponentTextureEntry> DefaultTextureEntries
    {
        get
        {
            ThrowIfNotInitialized();
            return _defaultTexturesRo;
        }
    }
    

    public IReadOnlyDictionary<GuiComponentType, ComponentTextureEntry> GetTextureEntries(string component, out bool componentExist)
    {
        if (!_perComponentTextures.TryGetValue(component, out var textures))
        {
            Logger?.LogDebug($"The component '{component}' has no overrides. Using default textures.");
            componentExist = false;
            return DefaultTextureEntries;
        }

        componentExist = true;
        return new ReadOnlyDictionary<GuiComponentType, ComponentTextureEntry>(textures);
    }
    
    public bool TryGetTextureEntry(string component, GuiComponentType key, out ComponentTextureEntry texture)
    {
        if (!_perComponentTextures.TryGetValue(component, out var textures))
        {
            Logger?.LogDebug($"The component '{component}' has no overrides. Using default textures.");
            textures = _defaultTextures;
        }

        return textures.TryGetValue(key, out texture);
    }
    
    public bool TextureExists(
        in ComponentTextureEntry textureInfo,
        out GuiTextureOrigin textureOrigin,
        out bool isNone,
        bool buttonMiddleInRepoMode = false)
    {
        if (textureInfo.Texture == "none")
        {
            textureOrigin = default;
            isNone = true;
            return false;
        }

        isNone = false;

        // Apparently, Scanlines only use the repository and not the MTD.
        if (textureInfo.ComponentType == GuiComponentType.Scanlines)
        {
            textureOrigin = GuiTextureOrigin.Repository;
            return GameRepository.TextureRepository.FileExists(textureInfo.Texture);
        }

        // The engine uses ButtonMiddle to switch to the special button mode.
        // It searches first in the repo and then falls back to MTD
        // (but only for this very type; the variants do not fallback to MTD).
        if (textureInfo.ComponentType == GuiComponentType.ButtonMiddle)
        {
            if (GameRepository.TextureRepository.FileExists(textureInfo.Texture))
            {
                textureOrigin = GuiTextureOrigin.Repository;
                return true;
            }
        }

        // The engine does not fallback to MTD once it is in this special Button mode.
        if (buttonMiddleInRepoMode && textureInfo.ComponentType is 
                GuiComponentType.ButtonMiddleDisabled or
                GuiComponentType.ButtonMiddleMouseOver or 
                GuiComponentType.ButtonMiddlePressed)
        {
            textureOrigin = GuiTextureOrigin.Repository;
            return GameRepository.TextureRepository.FileExists(textureInfo.Texture);
        }

        if (textureInfo.Texture.Length <= 63 && MtdFile is not null && _megaTextureExists)
        {
            var crc32 = _hashingService.GetCrc32Upper(textureInfo.Texture.AsSpan(), MtdFileConstants.NameEncoding);
            if (MtdFile.Content.Contains(crc32))
            {
                textureOrigin = GuiTextureOrigin.MegaTexture;
                return true;
            }
        }

        // The background image for frames include a fallback the repository.
        if (textureInfo.ComponentType == GuiComponentType.FrameBackground)
        {
            textureOrigin = GuiTextureOrigin.Repository;
            return GameRepository.FileExists(textureInfo.Texture);
        }

        textureOrigin = default;
        return false;
    }
}