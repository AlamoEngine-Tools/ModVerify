using AET.ModVerify.Reporting;
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

namespace AET.ModVerify.Verifiers.GuiDialogs;

sealed class GuiDialogsVerifier : GameVerifier
{
    internal const string DefaultComponentIdentifier = "<<DEFAULT>>";

    private static readonly GuiComponentType[] GuiComponentTypes =
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
            AddError(VerificationError.Create(this, VerifierErrorCodes.FileNotFound, $"MtdFile '{mtdFileName}.mtd' could not be found",
                VerificationSeverity.Critical, mtdFileName));
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
        var middleButtonInRepoMode = false;
        
        var entriesForComponent = GetTextureEntriesForComponents(component, out var defined);
        if (!defined)
            return;

        // TODO: Button Middle needs to be checked first as the engine checks this first

        foreach (var componentType in GuiComponentTypes)
        {
            try
            {
                if (!entriesForComponent.TryGetValue(componentType, out var texture))
                    continue;

                var cached = _cache?.GetEntry(texture.Texture);
                if (cached?.AlreadyVerified is true)
                {
                    // If we are in a special case we don't want to skip
                    if (!middleButtonInRepoMode &&
                        componentType is not GuiComponentType.ButtonMiddle &&
                        componentType is not GuiComponentType.Scanlines &&
                        componentType is not GuiComponentType.FrameBackground)
                        continue;
                }

                var exists = GameEngine.GuiDialogManager.TextureExists(
                    texture,
                    out var origin,
                    out var isNone,
                    middleButtonInRepoMode);
                
                if (!exists && !isNone)
                {
                    if (origin == GuiTextureOrigin.MegaTexture && texture.Texture.Length > MtdFileConstants.MaxFileNameSize)
                    {
                        AddError(VerificationError.Create(this, VerifierErrorCodes.FilePathTooLong,
                            $"The filename is too long. Max length is {MtdFileConstants.MaxFileNameSize} characters.",
                            VerificationSeverity.Error, texture.Texture));
                    }
                    else
                    {
                        AddNotFoundError(texture, component, origin);
                    }
                }

                if (componentType is GuiComponentType.ButtonMiddle && origin is GuiTextureOrigin.Repository)
                    middleButtonInRepoMode = true;

                _cache?.TryAddEntry(texture.Texture, exists);
            }
            finally
            {
                if (componentType >= GuiComponentType.ButtonRightDisabled)
                    middleButtonInRepoMode = false;
            }
        }
    }

    private void AddNotFoundError(ComponentTextureEntry texture, string component, GuiTextureOrigin? origin)
    {
        var sb = new StringBuilder($"Could not find GUI texture '{texture.Texture}'");
        if (origin is not null)
            sb.Append($" at location '{origin}'");
        sb.Append('.');

        if (texture.Texture.Length > PGConstants.MaxMegEntryPathLength)
            sb.Append(" The file name is too long.");

        AddError(VerificationError.Create(this, VerifierErrorCodes.FileNotFound,
            sb.ToString(), VerificationSeverity.Error,
            [component, origin.ToString()], texture.Texture));
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

    private void OnTextureError(object sender, VerificationErrorEventArgs e)
    {
        AddError(e.Error);
    }
}