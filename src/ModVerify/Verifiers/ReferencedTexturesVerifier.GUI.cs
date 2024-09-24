using System;
using System.Collections.Generic;
using System.Linq;
using AET.ModVerify.Reporting;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.GuiDialog;
using PG.StarWarsGame.Files.MTD.Binary;

namespace AET.ModVerify.Verifiers;

sealed partial class ReferencedTexturesVerifier
{
    internal const string DefaultComponentIdentifier = "<<DEFAULT>>";

    private static readonly GuiComponentType[] GuiComponentTypes =
        Enum.GetValues(typeof(GuiComponentType)).OfType<GuiComponentType>().ToArray();

    private void VerifyGuiTextures(ISet<string> visitedTextures)
    { 
        VerifyMegaTexturesExist();

        var components = new List<string>
        {
            DefaultComponentIdentifier
        };
        components.AddRange(Database.GuiDialogManager.Components);

        foreach (var component in components)
            VerifyGuiComponentTexturesExist(component, visitedTextures);

    }

    private void VerifyMegaTexturesExist()
    {
        var megaTextureName = Database.GuiDialogManager.GuiDialogsXml?.TextureData.MegaTexture;
        if (Database.GuiDialogManager.MtdFile is null)
        {
            var mtdFileName = megaTextureName ?? "<<MTD_NOT_SPECIFIED>>";
            VerificationError.Create(this, MtdNotFound, $"MtdFile '{mtdFileName}.mtd' could not be found",
                VerificationSeverity.Critical, mtdFileName);
        }

        
        if (megaTextureName is not null)
        {
            var megaTextureFileName = $"{megaTextureName}.tga";

            if (!Repository.TextureRepository.FileExists(megaTextureFileName))
            {
                VerificationError.Create(this, TexutreNotFound, $"Could not find texture '{megaTextureFileName}' could not be found",
                    VerificationSeverity.Error, megaTextureFileName);
            }
        }


        var compressedMegaTextureName = Database.GuiDialogManager.GuiDialogsXml?.TextureData.CompressedMegaTexture;
        if (compressedMegaTextureName is not null)
        {
            var compressedMegaTextureFieName = $"{compressedMegaTextureName}.dds";

            if (!Repository.TextureRepository.FileExists(compressedMegaTextureFieName))
            {
                VerificationError.Create(this, TexutreNotFound, $"Could not find texture '{compressedMegaTextureFieName}' could not be found",
                    VerificationSeverity.Error, compressedMegaTextureFieName);
            }
        }
    }

    private void VerifyGuiComponentTexturesExist(string component, ISet<string> visitedTextures)
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

                if (!visitedTextures.Add(texture.Texture))
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
                        AddError(VerificationError.Create(this, MtdIndexNameTooLong,
                            $"The filename is too long. Max length is {MtdFileConstants.MaxFileNameSize} characters.",
                            VerificationSeverity.Error, texture.Texture));
                    }
                    else if (origin == GuiTextureOrigin.Repository && texture.Texture.Length > PGConstants.MaxPathLength)
                    {
                        AddError(VerificationError.Create(this, MtdIndexNameTooLong,
                            $"The filename is too long. Max length is {PGConstants.MaxPathLength} characters.",
                            VerificationSeverity.Error, texture.Texture));
                    }
                    else
                    {
                        AddError(VerificationError.Create(this, TexutreNotFound,
                            $"Could not find GUI texture '{texture.Texture}' at location: {origin}", VerificationSeverity.Error,
                            texture.Texture, component, origin.ToString()));
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
}