using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine.Database.ErrorReporting;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Engine.Language;
using PG.StarWarsGame.Engine.Repositories;
using PG.StarWarsGame.Engine.Xml;

namespace PG.StarWarsGame.Engine.GameManagers;

internal class SfxEventGameManager(GameRepository repository, IServiceProvider serviceProvider)
    : GameManagerBase<SfxEvent>(repository, serviceProvider), ISfxEventGameManager
{
    private readonly IXmlContainerContentParser _contentParser =
        serviceProvider.GetRequiredService<IXmlContainerContentParser>();

    public IEnumerable<LanguageType> InstalledLanguages { get; private set; } = [];

    protected override async Task InitializeCoreAsync(DatabaseErrorListenerWrapper errorListener, CancellationToken token)
    {
        Logger?.LogInformation("Initializing Language files...");
        InstalledLanguages = GameRepository.InitializeInstalledSfxMegFiles().ToList();
        Logger?.LogInformation("Finished initializing Language files");

        Logger?.LogInformation("Parsing SFXEvents...");
        await Task.Run(() => _contentParser.ParseEntriesFromContainerXml("DATA\\XML\\SFXEventFiles.XML", errorListener, GameRepository, NamedEntries), token);
    }
}