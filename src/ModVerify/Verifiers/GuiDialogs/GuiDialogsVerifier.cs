using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers.Caching;
using AET.ModVerify.Verifiers.Commons;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.GuiDialog;
using PG.StarWarsGame.Files.MTD.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AET.ModVerify.Reporting.Diagnostics;

namespace AET.ModVerify.Verifiers.GuiDialogs;

public sealed class GuiDialogsVerifier : GameVerifier
{
    internal const string DefaultComponentIdentifier = "<<DEFAULT>>";

    private static readonly IReadOnlyList<GuiComponentType> GuiComponentTypes =
        Enum.GetValues(typeof(GuiComponentType)).OfType<GuiComponentType>().ToArray();

    private readonly IAlreadyVerifiedCache? _cache;
    private readonly TextureVerifier _textureVerifier;

    public GuiDialogsVerifier(
        IStarWarsGameEngine gameEngine,
        GameVerifySettings settings,
        IServiceProvider serviceProvider) 
        : base(gameEngine, settings, serviceProvider)
    {
        _cache = serviceProvider.GetService<IAlreadyVerifiedCache>();
        _textureVerifier = new TextureVerifier(this);
    }

    public override void Verify(CancellationToken token)
    {
        VerifyMegaTexturesExist(token);
        VerifyGuiTextures();

        foreach (var textureError in _textureVerifier.VerifyErrors)
            AddError(textureError);
    }

    private void VerifyGuiTextures()
    { 
        var components = new List<string>
        {
            DefaultComponentIdentifier
        };
        components.AddRange(GameEngine.GuiDialogManager.Components);

        // TODO: Verify no double definitions for textures and components exit
        
        foreach (var component in components)
            VerifyGuiComponentTexturesExist(component);

    }

    private void VerifyMegaTexturesExist(CancellationToken token)
    {
        var megaTextureName = GameEngine.GuiDialogManager.GuiDialogsXml?.TextureData.MegaTexture;
        if (GameEngine.GuiDialogManager.MtdFile is null)
        {
            var mtdFileName = megaTextureName ?? "<<MTD_NOT_SPECIFIED>>";
            AddError(GuiDialogErrors.MtdFileNotFound(this, mtdFileName, []));
        }

        if (megaTextureName is not null)
        {
            var megaTextureFileName = $"{megaTextureName}.tga";
            _textureVerifier.Verify(megaTextureFileName, ["GUIDIALOGS.XML"], token);
        }


        var compressedMegaTextureName = GameEngine.GuiDialogManager.GuiDialogsXml?.TextureData.CompressedMegaTexture;
        if (compressedMegaTextureName is not null)
        {
            var compressedMegaTextureFieName = $"{compressedMegaTextureName}.dds";
            _textureVerifier.Verify(compressedMegaTextureFieName, ["GUIDIALOGS.XML"], token);
        }
    }

    private void VerifyGuiComponentTexturesExist(string component)
    {
        var buttonSpecialMode = false;
        
        var entriesForComponent = GetTextureEntriesForComponents(component, out var defined);
        if (!defined)
            return;

        if (entriesForComponent.TryGetValue(GuiComponentType.ButtonMiddle, out var middleTexture))
        {
            GameEngine.GuiDialogManager.TextureExists(middleTexture, out var origin, out _);
            if (origin == GuiTextureOrigin.Repository)
                buttonSpecialMode = true;
        }

        foreach (var componentType in GuiComponentTypes)
        {
            try
            {
                if (!entriesForComponent.TryGetValue(componentType, out var texture))
                    continue;

                if (buttonSpecialMode && componentType.IsButton() && !componentType.SupportsSpecialTextureMode())
                {
                    // If we are in special button mode, non-supported button textures won't be loaded anyway.
                    continue;
                }

                if (texture.Texture.Equals("none", StringComparison.OrdinalIgnoreCase))
                {
                    // We can ignore "none" textures completely, due to two reasons:
                    // 1. If we are in special mode, the engine already filters for "none" textures and ignores them.
                    // 2. If we are in MegaTexture mode, the texture is rendered as a view from the mega texture.
                    //    When the engine does not find a texture in the MegaTexture, the view becomes a rect of (0,0,0,0)
                    //    and thus does not render anything, which is the intended effect of "none" textures.
                    //    The engine does not log any warnings for missing textures in the MegaTexture, so we won't either.
                    continue;
                }
                
                var cached = _cache?.GetEntry(texture.Texture);
                if (cached?.AlreadyVerified is true)
                {
                    // If we are in a special case we don't want to skip
                    if (!buttonSpecialMode &&
                        componentType is not GuiComponentType.ButtonMiddle &&
                        componentType is not GuiComponentType.Scanlines &&
                        componentType is not GuiComponentType.FrameBackground)
                    {
                        if (!cached.Value.AssetExists) 
                            AddNotFoundError(texture, component, null);
                        continue;
                    }
                }

                var exists = GameEngine.GuiDialogManager.TextureExists(
                    texture,
                    out var origin,
                    out var isNone,
                    buttonSpecialMode);
                
                if (!exists && !isNone)
                {
                    if (origin == GuiTextureOrigin.MegaTexture && texture.Texture.Length > MtdFileConstants.MaxFileNameSize)
                    {
                        AddError(GuiDialogErrors.TextureNameTooLong(this, texture.Texture, MtdFileConstants.MaxFileNameSize, []));
                    }
                    else
                    {
                        AddNotFoundError(texture, component, origin);
                    }
                }

                // If the texture is "none" we store it as "asset exists" in order to reduce false warnings
                _cache?.TryAddEntry(texture.Texture, exists || isNone);
            }
            finally
            {
                if (!componentType.IsButton())
                    buttonSpecialMode = false;
            }
        }
    }

    private void AddNotFoundError(ComponentTextureEntry texture, string component, GuiTextureOrigin? origin)
    {
        var sb = new StringBuilder($"Could not find GUI texture '{texture.Texture}' of type '{texture.ComponentType}'");
        if (origin is not null) 
            sb.Append($" at origin '{origin}'");
        sb.Append($" for component '{component}'");
        sb.Append('.');

        if (texture.Texture.Length > PGConstants.MaxMegEntryPathLength)
            sb.Append(" The file name is too long.");

        // Origin is not interesting for context, but might be for the error message
        AddError(GuiDialogErrors.GuiTextureNotFound(this, texture.Texture, sb.ToString(), [component]));
    }

    private IReadOnlyDictionary<GuiComponentType, ComponentTextureEntry> GetTextureEntriesForComponents(string component, out bool defined)
    {
        if (component == DefaultComponentIdentifier)
        {
            defined = true;
            return GameEngine.GuiDialogManager.DefaultTextureEntries;
        }
        return GameEngine.GuiDialogManager.GetTextureEntries(component, out defined);
    }
}