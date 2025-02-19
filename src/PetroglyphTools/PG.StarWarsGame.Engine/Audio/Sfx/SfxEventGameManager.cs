using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO.Repositories;
using PG.StarWarsGame.Engine.Localization;
using PG.StarWarsGame.Engine.Xml.Parsers;

namespace PG.StarWarsGame.Engine.Audio.Sfx;

internal class SfxEventGameManager(GameRepository repository, GameErrorReporterWrapper errorReporter, IServiceProvider serviceProvider)
    : GameManagerBase<SfxEvent>(repository, errorReporter, serviceProvider), ISfxEventGameManager
{
    public IEnumerable<LanguageType> InstalledLanguages { get; private set; } = [];

    protected override async Task InitializeCoreAsync(CancellationToken token)
    {
        Logger?.LogInformation("Initializing Language files...");
        InstalledLanguages = GameRepository.InitializeInstalledSfxMegFiles().ToList();
        Logger?.LogInformation("Finished initializing Language files");

        Logger?.LogInformation("Parsing SFXEvents...");

        var contentParser = new XmlContainerContentParser(ServiceProvider, ErrorReporter);
        contentParser.XmlParseError += OnParseError;
        try
        {
            await Task.Run(() => contentParser.ParseEntriesFromFileListXml(
                    "DATA\\XML\\SFXEventFiles.XML",
                    GameRepository,
                    "DATA\\XML",
                    NamedEntries,
                    VerifyFilePathLength),
                token);
        }
        finally
        {
            contentParser.XmlParseError -= OnParseError;
        }
    }

    private void OnParseError(object sender, XmlContainerParserErrorEventArgs e)
    {
        if (e.ErrorInXmlFileList || e.HasException)
        {
            e.Continue = false;
            ErrorReporter.Report(new InitializationError
            {
                GameManager = ToString(),
                Message = GetMessage(e)
            });
        }
    }

    private static string GetMessage(XmlContainerParserErrorEventArgs errorEventArgs)
    {
        if (errorEventArgs.HasException)
            return $"Error while parsing SFXEvent XML file '{errorEventArgs.File}': {errorEventArgs.Exception.Message}";
        return "Could not find SFXEventFiles.xml";
    }

    private void VerifyFilePathLength(string filePath)
    {
        if (filePath.Length > PGConstants.MaxSFXEventDatabaseFileName)
        {
            ErrorReporter.Report(new InitializationError
            {
                GameManager = ToString(),
                Message = $"SFXEvent file '{filePath}' is longer than {PGConstants.MaxSFXEventDatabaseFileName} characters."
            });
        }
    }
}