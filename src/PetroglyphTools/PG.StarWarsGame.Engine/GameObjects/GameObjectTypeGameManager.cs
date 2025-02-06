using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Engine.Database.ErrorReporting;
using PG.StarWarsGame.Engine.IO.Repositories;
using PG.StarWarsGame.Engine.Xml.Parsers;

namespace PG.StarWarsGame.Engine.GameObjects;

internal class GameObjectTypeGameManager(GameRepository repository, DatabaseErrorReporterWrapper errorReporter, IServiceProvider serviceProvider)
    : GameManagerBase<GameObject>(repository, errorReporter, serviceProvider), IGameObjectTypeGameManager
{
    protected override async Task InitializeCoreAsync(CancellationToken token)
    {
        Logger?.LogInformation("Parsing GameObjects...");

        var contentParser = ServiceProvider.GetRequiredService<IXmlContainerContentParser>();

        await Task.Run(() => contentParser.ParseEntriesFromFileListXml(
            "DATA\\XML\\GAMEOBJECTFILES.XML",
            ErrorReporter,
            GameRepository,
            ".\\DATA\\XML",
            NamedEntries,
            VerifyFilePathLength), token);
    }

    private void VerifyFilePathLength(string filePath)
    {
        if (filePath.Length > PGConstants.MaxGameObjectDatabaseFileName)
        {
            ErrorReporter.Report(new InitializationError
            {
                GameManager = ToString(),
                Message = $"Game object file '{filePath}' is longer than {PGConstants.MaxGameObjectDatabaseFileName} characters."
            });
        }
    }
}