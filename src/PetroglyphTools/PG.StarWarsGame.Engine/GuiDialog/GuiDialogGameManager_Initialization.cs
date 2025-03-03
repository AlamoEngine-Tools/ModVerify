using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.CommonUtilities.Collections;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.GuiDialog.Xml;
using PG.StarWarsGame.Engine.Xml.Parsers.File;
using PG.StarWarsGame.Engine.Xml.Tags;
using PG.StarWarsGame.Files.Binary;

namespace PG.StarWarsGame.Engine.GuiDialog;

partial class GuiDialogGameManager
{
    public const int MegaTextureMaxFilePathLength = 255;

    protected override Task InitializeCoreAsync(CancellationToken token)
    {
        return Task.Run(() =>
        {
            var guiDialogParser = new GuiDialogParser(ServiceProvider, ErrorReporter);

            _defaultTexturesRo = new ReadOnlyDictionary<GuiComponentType, ComponentTextureEntry>(_defaultTextures);

            Logger?.LogInformation("Parsing GuiDialogs...");
            using var fileStream = GameRepository.TryOpenFile("DATA\\XML\\GUIDIALOGS.XML");

            if (fileStream is null)
            {
                ErrorReporter.Report(new InitializationError
                {
                    GameManager = ToString(),
                    Message = "Unable to find GuiDialogs.xml"
                });
                return;
            }

            var guiDialogs = guiDialogParser.ParseFile(fileStream);
            if (guiDialogs is null)
            {
                ErrorReporter.Report(new InitializationError
                {
                    GameManager = ToString(),
                    Message = "Unable to parse GuiDialogs.xml"
                });
                return;
            }

            GuiDialogsXml = guiDialogs;

            InitializeTextures(guiDialogs.TextureData);

        }, token);
    }

    private void InitializeTextures(GuiDialogsXmlTextureData textureData)
    {
        InitializeMegaTextures(textureData);

        var textures = textureData.Textures;

        if (textures.Count == 0)
        {
            ErrorReporter.Report(new InitializationError
            {
                GameManager = ToString(),
                Message = "No Textures defined in GuiDialogs.xml"
            });
        }
        else
        {
            var defaultCandidate = textures.First();

            // Regardless of its name, the game treats the first entry as default.
            var defaultTextures = InitializeComponentTextures(defaultCandidate, true, out var invalidKeys);
            foreach (var entry in defaultTextures)
                _defaultTextures.Add(entry.Key, entry.Value);

            ReportInvalidComponent(in invalidKeys);
        }

        foreach (var componentTextureData in textures.Skip(1))
        {
            // The game only uses the *first* entry.
            if (_perComponentTextures.ContainsKey(componentTextureData.Component))
                continue;

            _perComponentTextures.Add(componentTextureData.Component, InitializeComponentTextures(componentTextureData, false, out var invalidKeys));
            ReportInvalidComponent(in invalidKeys);
        }
    }

    private Dictionary<GuiComponentType, ComponentTextureEntry> InitializeComponentTextures(XmlComponentTextureData textureData, bool isDefaultComponent, out FrugalList<string> invalidKeys)
    {
        invalidKeys = new FrugalList<string>();

        var result = new Dictionary<GuiComponentType, ComponentTextureEntry>();

        if (!isDefaultComponent)
        {
            // This assumes that _defaultTextures is already filled
            foreach (var key in _defaultTextures.Keys)
                result.Add(key, _defaultTextures[key]);
        }


        foreach (var keyText in textureData.Textures.Keys)
        {
            if (!ComponentTextureKeyExtensions.TryConvertToKey(keyText.AsSpan(), out var key))
            {
                invalidKeys.Add(keyText);
                continue;
            }

            var textureValue = textureData.Textures.GetLastValue(keyText);
            result[key] = new ComponentTextureEntry(key, textureValue, !isDefaultComponent);
        }

        return result;
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
                ErrorReporter.Assert(EngineAssert.Create(EngineAssertKind.CorruptBinary, mtdPath, null, message));
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