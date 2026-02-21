using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO.Repositories;
using PG.StarWarsGame.Engine.Localization;
using PG.StarWarsGame.Engine.Xml;

namespace PG.StarWarsGame.Engine.Audio.Sfx;

internal class SfxEventGameManager(GameRepository repository, GameEngineErrorReporterWrapper errorReporter, IServiceProvider serviceProvider)
    : GameManagerBase<SfxEvent>(repository, errorReporter, serviceProvider), ISfxEventGameManager
{
    public IEnumerable<LanguageType> InstalledLanguages { get; private set; } = [];

    protected override async Task InitializeCoreAsync(CancellationToken token)
    {
        Logger?.LogInformation("Initializing Language files...");
        InstalledLanguages = GameRepository.InitializeInstalledSfxMegFiles().ToList();
        Logger?.LogInformation("Finished initializing Language files");

        Logger?.LogInformation("Parsing SFXEvents...");

        var contentParser = new PetroglyphStarWarsGameXmlParser(GameRepository,
            new PetroglyphStarWarsGameXmlParseSettings
            {
                GameManager = ToString(),
                InvalidObjectXmlFailsInitialization = true,
                InvalidFilesListXmlFailsInitialization = true
            }, ServiceProvider, ErrorReporter);

        await Task.Run(() => contentParser.ParseEntriesFromFileListXml(
                "DATA\\XML\\SFXEventFiles.XML",
                "DATA\\XML",
                NamedEntries,
                VerifyFilePathLength),
            token);
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