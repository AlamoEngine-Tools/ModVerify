using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO.Repositories;
using PG.StarWarsGame.Engine.Xml.Parsers;

namespace PG.StarWarsGame.Engine.GameObjects;

internal class GameObjectTypeGameManager(GameRepository repository, GameEngineErrorReporterWrapper errorReporter, IServiceProvider serviceProvider)
    : GameManagerBase<GameObject>(repository, errorReporter, serviceProvider), IGameObjectTypeGameManager
{
    protected override async Task InitializeCoreAsync(CancellationToken token)
    {
        Logger?.LogInformation("Parsing GameObjects...");

        var contentParser = new XmlContainerContentParser(ServiceProvider, ErrorReporter);

        contentParser.XmlParseError += OnParseError;

        try
        {
            await Task.Run(() => contentParser.ParseEntriesFromFileListXml(
                "DATA\\XML\\GAMEOBJECTFILES.XML",
                GameRepository,
                ".\\DATA\\XML\\",
                NamedEntries,
                VerifyFilePathLength), token);
        }
        finally
        {
            contentParser.XmlParseError -= OnParseError;
        }
    }

    private void OnParseError(object sender, XmlContainerParserErrorEventArgs e)
    {
        if (e.ErrorInXmlFileList)
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
            return $"Error while parsing XML file '{errorEventArgs.File}': {errorEventArgs.Exception.Message}";
        return "Could not find GameObjectFiles.xml";
    }

    private void VerifyFilePathLength(string filePath)
    {
        if (filePath.Length > PGConstants.MaxGameObjectDatabaseFileName)
        {
            // Technically this is an assert in the engine, but in Release Mode, the game CTDs.
            // Thus, we rank this as an initialization error.
            ErrorReporter.Report(new InitializationError
            {
                GameManager = ToString(),
                Message = $"Game object file '{filePath}' is longer than {PGConstants.MaxGameObjectDatabaseFileName} characters."
            });
        }
    }
}