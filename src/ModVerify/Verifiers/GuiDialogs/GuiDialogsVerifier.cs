using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers.Commons;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Engine.GuiDialog;
using PG.StarWarsGame.Files.MTD.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AET.ModVerify.Verifiers.GuiDialogs;

sealed class GuiDialogsVerifier : GameVerifier
{
    internal const string DefaultComponentIdentifier = "<<DEFAULT>>";

    private static readonly GuiComponentType[] GuiComponentTypes =
        Enum.GetValues(typeof(GuiComponentType)).OfType<GuiComponentType>().ToArray();

    private readonly IAlreadyVerifiedCache? _cache;
    private readonly TextureVeifier _textureVerifier;

    public GuiDialogsVerifier(IGameDatabase gameDatabase,
        GameVerifySettings settings,
        IServiceProvider serviceProvider) : base(null, gameDatabase, settings, serviceProvider)
    {
        _cache = serviceProvider.GetService<IAlreadyVerifiedCache>();
        _textureVerifier = new TextureVeifier(this, gameDatabase, settings, serviceProvider);
    }

    public override void Verify(CancellationToken token)
    {
        try
        {
            _textureVerifier.Error += OnTextureError;
            VerifyMegaTexturesExist(token);
            VerifyGuiTextures();
        }
        finally
        {
            _textureVerifier.Error -= OnTextureError;
        }
    }

    private void VerifyGuiTextures()
    { 
        var components = new List<string>
        {
            DefaultComponentIdentifier
        };
        components.AddRange(Database.GuiDialogManager.Components);

        foreach (var component in components)
            VerifyGuiComponentTexturesExist(component);

    }

    private void VerifyMegaTexturesExist(CancellationToken token)
    {
        var megaTextureName = Database.GuiDialogManager.GuiDialogsXml?.TextureData.MegaTexture;
        if (Database.GuiDialogManager.MtdFile is null)
        {
            var mtdFileName = megaTextureName ?? "<<MTD_NOT_SPECIFIED>>";
            VerificationError.Create(VerifierChain, VerifierErrorCodes.FileNotFound, $"MtdFile '{mtdFileName}.mtd' could not be found",
                VerificationSeverity.Critical, mtdFileName);
        }

        if (megaTextureName is not null)
        {
            var megaTextureFileName = $"{megaTextureName}.tga";
            _textureVerifier.Verify(megaTextureFileName, ["GUIDIALOGS.XML"], token);
        }


        var compressedMegaTextureName = Database.GuiDialogManager.GuiDialogsXml?.TextureData.CompressedMegaTexture;
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

        foreach (var componentType in GuiComponentTypes)
        {
            try
            {
                if (!entriesForComponent.TryGetValue(componentType, out var texture))
                    continue;

                if (_cache?.TryAddEntry(texture.Texture) == false)
                {
                    // If we are in a special case we don't want to skip
                    if (!middleButtonInRepoMode &&
                        componentType is not GuiComponentType.ButtonMiddle &&
                        componentType is not GuiComponentType.Scanlines &&
                        componentType is not GuiComponentType.FrameBackground)
                        continue;
                }

                if (!Database.GuiDialogManager.TextureExists(
                        texture,
                        out var origin,
                        out var isNone,
                        middleButtonInRepoMode)
                    && !isNone)
                {

                    if (origin == GuiTextureOrigin.MegaTexture && texture.Texture.Length > MtdFileConstants.MaxFileNameSize)
                    {
                        AddError(VerificationError.Create(VerifierChain, VerifierErrorCodes.FilePathTooLong,
                            $"The filename is too long. Max length is {MtdFileConstants.MaxFileNameSize} characters.",
                            VerificationSeverity.Error, texture.Texture));
                    }
                    else
                    {
                        var message = $"Could not find GUI texture '{texture.Texture}' at location '{origin}'.";
                        
                        if (texture.Texture.Length > PGConstants.MaxMegEntryPathLength)
                            message += " The file name is too long.";

                        AddError(VerificationError.Create(VerifierChain, VerifierErrorCodes.FileNotFound,
                            message, VerificationSeverity.Error,
                            [component, origin.ToString()], texture.Texture));
                    }
                }

                if (componentType is GuiComponentType.ButtonMiddle && origin is GuiTextureOrigin.Repository)
                    middleButtonInRepoMode = true;
            }
            finally
            {

                if (componentType >= GuiComponentType.ButtonRightDisabled)
                    middleButtonInRepoMode = false;
            }
        }
    }

    private IReadOnlyDictionary<GuiComponentType, ComponentTextureEntry> GetTextureEntriesForComponents(string component, out bool defined)
    {
        if (component == DefaultComponentIdentifier)
        {
            defined = true;
            return Database.GuiDialogManager.DefaultTextureEntries;
        }
        return Database.GuiDialogManager.GetTextureEntries(component, out defined);
    }

    private void OnTextureError(object sender, VerificationErrorEventArgs e)
    {
        AddError(e.Error);
    }
}