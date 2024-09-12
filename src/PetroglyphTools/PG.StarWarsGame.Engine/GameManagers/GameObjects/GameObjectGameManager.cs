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

internal class GameObjectGameManager(GameRepository repository, DatabaseErrorListenerWrapper errorListener, IServiceProvider serviceProvider)
    : GameManagerBase<GameObject>(repository, errorListener, serviceProvider), IGameObjectGameManager
{
    private readonly IXmlContainerContentParser _contentParser =
        serviceProvider.GetRequiredService<IXmlContainerContentParser>();

    protected override async Task InitializeCoreAsync(CancellationToken token)
    {
        Logger?.LogInformation("Parsing GameObjects...");
        await Task.Run(() => _contentParser.ParseEntriesFromContainerXml("DATA\\XML\\GAMEOBJECTFILES.XML", ErrorListener, GameRepository, NamedEntries), token);
    }
}