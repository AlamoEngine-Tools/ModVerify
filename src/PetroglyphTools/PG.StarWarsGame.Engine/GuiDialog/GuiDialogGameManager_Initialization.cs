using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.CommonUtilities.Collections;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.GuiDialog.Xml;
using PG.StarWarsGame.Engine.Xml;
using PG.StarWarsGame.Engine.Xml.Parsers;
using PG.StarWarsGame.Files.Binary;

namespace PG.StarWarsGame.Engine.GuiDialog;

internal partial class GuiDialogGameManager
{
    public const int MegaTextureMaxFilePathLength = 255;

    protected override Task InitializeCoreAsync(CancellationToken token)
    {
        return Task.Run(() =>
        {
            var engineParser = new PetroglyphStarWarsGameXmlParser(GameRepository,
                new PetroglyphStarWarsGameXmlParseSettings
                {
                    GameManager = ToString(),
                    InvalidObjectXmlFailsInitialization = true
                }, ServiceProvider, ErrorReporter);

            var guiDialogs = engineParser.ParseFile("DATA\\XML\\GUIDIALOGS.XML", new GuiDialogParser(ServiceProvider, ErrorReporter));
            if (guiDialogs is null)
            {
                ErrorReporter.Report(new InitializationError
                {
                    GameManager = ToString(),
                    Message = "Unable to parse GuiDialogs.xml"
                });
                return;
            }

            InitializeTextures(guiDialogs.TextureData);
            GuiDialogsXml = guiDialogs;
        }, token);
    }

    private void InitializeTextures(GuiDialogsXmlTextureData textureData)
    {
        InitializeMegaTextures(textureData);

        var textures = textureData.Textures;
        
        IReadOnlyDictionary<GuiComponentType, ComponentTextureEntry> defaultTextures;
        if (textures.Count == 0)
        {
            ErrorReporter.Report(new InitializationError
            {
                GameManager = ToString(),
                Message = "No Textures defined in GuiDialogs.xml"
            });

            defaultTextures = new ReadOnlyDictionary<GuiComponentType, ComponentTextureEntry>(
                    new Dictionary<GuiComponentType, ComponentTextureEntry>());
        }
        else
        {
            // Regardless of its name, the game treats the first entry as default.
            var defaultCandidate = textures.First();

            defaultTextures = InitializeComponentTextures(defaultCandidate, null, out var invalidKeys);
           
            ReportInvalidComponent(in invalidKeys);
        }

        var perComponentTextures = new Dictionary<string, IReadOnlyDictionary<GuiComponentType, ComponentTextureEntry>>();

        foreach (var componentTextureData in textures.Skip(1))
        {
            // The game only uses the *first* entry.
            if (perComponentTextures.ContainsKey(componentTextureData.Component))
                continue;

            perComponentTextures.Add(
                componentTextureData.Component, 
                InitializeComponentTextures(componentTextureData, defaultTextures, out var invalidKeys));

            ReportInvalidComponent(in invalidKeys);
        }

        DefaultTextureEntries = defaultTextures;
        PerComponentTextures =
            new ReadOnlyDictionary<string, IReadOnlyDictionary<GuiComponentType, ComponentTextureEntry>>(
                perComponentTextures);
    }

    private ReadOnlyDictionary<GuiComponentType, ComponentTextureEntry> InitializeComponentTextures(
        XmlComponentTextureData textureData,
        IReadOnlyDictionary<GuiComponentType, ComponentTextureEntry>? defaultTextures, 
        out FrugalList<string> invalidKeys)
    {
        invalidKeys = [];

        var result = new Dictionary<GuiComponentType, ComponentTextureEntry>();

        var isDefaultComponent = defaultTextures is null;

        if (!isDefaultComponent)
        {
            foreach (var key in defaultTextures!.Keys)
                result.Add(key, defaultTextures[key]);
        }

        if (textureData.Textures.Count == 0)
        {
            ErrorReporter.Report(new InitializationError
            {
                GameManager = ToString(),
                Message = $"No Textures defined for component '{textureData.Component}' in GuiDialogs.xml"
            });
        }
        
        foreach (var keyText in textureData.Textures.Keys)
        {
            if (!GuiDialogParser.ComponentTypeDictionary.TryStringToEnum(keyText, out var key))
            {
                invalidKeys.Add(keyText);
                continue;
            }

            var textureValue = textureData.Textures.GetLastValue(keyText);
            result[key] = new ComponentTextureEntry(key, textureValue, !isDefaultComponent);
        }
        
        return new ReadOnlyDictionary<GuiComponentType, ComponentTextureEntry>(result);
    }

    private void InitializeMegaTextures(GuiDialogsXmlTextureData guiDialogs)
    {
        if (guiDialogs.MegaTexture is null)
        {
            ErrorReporter.Report(new InitializationError
            {
                GameManager = ToString(),
                Message = "MtdFile is not defined in GuiDialogs.xml"
            });
        }
        else
        {
            var mtdPath = FileSystem.Path.Combine("DATA\\ART\\TEXTURES", $"{guiDialogs.MegaTexture}.mtd");

            if (mtdPath.Length > MegaTextureMaxFilePathLength)
            {
                ErrorReporter.Report(new InitializationError
                {
                    GameManager = ToString(),
                    Message = $"Mtd file path is longer than {MegaTextureMaxFilePathLength}."
                });
            }

            using var megaTexture = GameRepository.TryOpenFile(mtdPath);
            try
            {
                MtdFile = megaTexture is null ? null : _mtdFileService.Load(megaTexture);
            }
            catch (BinaryCorruptedException e)
            {
                var message = $"Failed to load MTD file '{mtdPath}': {e.Message}";
                Logger?.LogError(e, message);
                ErrorReporter.Assert(EngineAssert.Create(EngineAssertKind.CorruptBinary, mtdPath, [], message));
            }
        }

        if (guiDialogs.CompressedMegaTexture is null)
        {
            ErrorReporter.Report(new InitializationError
            {
                GameManager = ToString(),
                Message = "CompressedMegaTexture is not defined in GuiDialogs.xml"
            });
        }


        // TODO: Support using the correct texture based on desired low-RAM flag
        _megaTextureFileName = guiDialogs.MegaTexture;
        var textureFileNameWithExtension = $"{guiDialogs.MegaTexture}.tga";
        _megaTextureExists = GameRepository.TextureRepository.FileExists(textureFileNameWithExtension);

        if (textureFileNameWithExtension.Length > MegaTextureMaxFilePathLength)
        {
            ErrorReporter.Report(new InitializationError
            {
                GameManager = ToString(),
                Message = $"MegaTexture file path is longer than {MegaTextureMaxFilePathLength}."
            });
        }
    }

    private void ReportInvalidComponent(in FrugalList<string> invalidKeys)
    {
        if (invalidKeys.Count == 0)
            return;

        ErrorReporter.Report(new InitializationError
        {
            GameManager = ToString(),
            Message = $"The following XML keys are not valid to describe a GUI component: {string.Join(",", invalidKeys)}"
        });
    }
}