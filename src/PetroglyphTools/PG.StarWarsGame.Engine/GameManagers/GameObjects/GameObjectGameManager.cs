using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine.Database.ErrorReporting;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Engine.Repositories;
using PG.StarWarsGame.Engine.Xml;

namespace PG.StarWarsGame.Engine.GameManagers;

internal class GameObjectGameManager(GameRepository repository, IServiceProvider serviceProvider)
    : GameManagerBase<GameObject>(repository, serviceProvider), IGameObjectGameManager
{
    private readonly IXmlContainerContentParser _contentParser =
        serviceProvider.GetRequiredService<IXmlContainerContentParser>();

    protected override async Task InitializeCoreAsync(DatabaseErrorListenerWrapper errorListener, CancellationToken token)
    {
        Logger?.LogInformation("Parsing GameObjects...");
        await Task.Run(() => _contentParser.ParseEntriesFromContainerXml("DATA\\XML\\GAMEOBJECTFILES.XML", errorListener, GameRepository, NamedEntries), token);
    }
}