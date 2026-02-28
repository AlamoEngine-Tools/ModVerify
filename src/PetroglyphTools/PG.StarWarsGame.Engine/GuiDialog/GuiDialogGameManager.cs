using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.GuiDialog.Xml;
using PG.StarWarsGame.Engine.IO.Repositories;
using PG.StarWarsGame.Files.MTD.Binary;
using PG.StarWarsGame.Files.MTD.Files;
using PG.StarWarsGame.Files.MTD.Services;

namespace PG.StarWarsGame.Engine.GuiDialog;

internal partial class GuiDialogGameManager(
    GameEngineType engineType,
    GameRepository repository, 
    GameEngineErrorReporterWrapper errorReporter,
    IServiceProvider serviceProvider)
    : GameManagerBase(engineType, repository, errorReporter, serviceProvider), IGuiDialogManager
{
    private readonly IMtdFileService _mtdFileService = serviceProvider.GetRequiredService<IMtdFileService>();
    private readonly ICrc32HashingService _hashingService = serviceProvider.GetRequiredService<ICrc32HashingService>();

    private bool _megaTextureExists;
    private string? _megaTextureFileName;

    [field: MaybeNull, AllowNull]
    private ReadOnlyDictionary<string, IReadOnlyDictionary<GuiComponentType, ComponentTextureEntry>> PerComponentTextures
    {
        get
        {
            ThrowIfNotInitialized();
            return field!;
        }
        set
        {
            ThrowIfAlreadyInitialized();
            field = value;
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
            return PerComponentTextures.Keys!;
        }
    }

    public IReadOnlyDictionary<GuiComponentType, ComponentTextureEntry> DefaultTextureEntries
    {
        get
        {
            ThrowIfNotInitialized();
            return field!;
        }
        internal set
        {
            ThrowIfAlreadyInitialized();
            field = value;
        }
    }
    

    public IReadOnlyDictionary<GuiComponentType, ComponentTextureEntry> GetTextureEntries(string component, out bool componentExist)
    {
        if (!PerComponentTextures.TryGetValue(component, out var textures))
        {
            Logger?.LogDebug("The component '{Component}' has no overrides. Using default textures.", component);
            componentExist = false;
            return DefaultTextureEntries;
        }

        componentExist = true;
        return textures;
    }
    
    public bool TryGetTextureEntry(string component, GuiComponentType key, out ComponentTextureEntry texture)
    {
        if (!PerComponentTextures.TryGetValue(component, out var textures))
        {
            Logger?.LogDebug("The component '{Component}' has no overrides. Using default textures.", component);
            textures = DefaultTextureEntries;
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
            return GameRepository.TextureRepository.FileExists(textureInfo.Texture);
        }

        textureOrigin = default;
        return false;
    }
}