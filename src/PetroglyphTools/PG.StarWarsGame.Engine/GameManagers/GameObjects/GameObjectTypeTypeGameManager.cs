using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine.Database.ErrorReporting;
using PG.StarWarsGame.Engine.GameObjects;
using PG.StarWarsGame.Engine.Repositories;
using PG.StarWarsGame.Engine.Xml;

namespace PG.StarWarsGame.Engine.GameManagers;

internal class GameObjectTypeTypeGameManager(GameRepository repository, DatabaseErrorListenerWrapper errorListener, IServiceProvider serviceProvider)
    : GameManagerBase<GameObject>(repository, errorListener, serviceProvider), IGameObjectTypeGameManager
{
    private readonly IXmlContainerContentParser _contentParser = serviceProvider.GetRequiredService<IXmlContainerContentParser>();

    private const int MaxPathLength = 127;

    protected override async Task InitializeCoreAsync(CancellationToken token)
    {
        Logger?.LogInformation("Parsing GameObjects...");

        await Task.Run(() => _contentParser.ParseEntriesFromContainerXml(
            "DATA\\XML\\GAMEOBJECTFILES.XML",
            ErrorListener,
            GameRepository,
            ".\\DATA\\XML",
            NamedEntries,
            VerifyFilePathLength), token);
    }

    private void VerifyFilePathLength(string filePath)
    {
        if (filePath.Length > MaxPathLength)
        {
            ErrorListener.OnInitializationError(new InitializationError
            {
                GameManager = ToString(),
                Message = $"Game object file '{filePath}' is longer than {MaxPathLength} characters."
            });
        }
    }
}