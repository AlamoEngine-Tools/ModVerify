using System;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.CommonUtilities;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.Database.Initialization;
using PG.StarWarsGame.Engine.Repositories;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Database;

internal class GameDatabaseService : DisposableObject, IXmlParserErrorListener, IGameDatabaseService
{
    public event XmlErrorEventHandler? XmlParseError;

    private readonly IServiceProvider _serviceProvider;
    private IPrimitiveXmlErrorParserProvider _primitiveXmlParserErrorProvider;

    public GameDatabaseService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _primitiveXmlParserErrorProvider = serviceProvider.GetRequiredService<IPrimitiveXmlErrorParserProvider>();
        _primitiveXmlParserErrorProvider.XmlParseError += OnXmlParseError;
    }
    
    public async Task<IGameDatabase> CreateDatabaseAsync(
        GameEngineType targetEngineType, 
        GameLocations locations,
        CancellationToken cancellationToken = default)
    {
        var repoFactory = _serviceProvider.GetRequiredService<IGameRepositoryFactory>();
        var repository = repoFactory.Create(targetEngineType, locations, this);

        var pipeline = new GameDatabaseCreationPipeline(repository, this, _serviceProvider);
        await pipeline.RunAsync(cancellationToken);
        return pipeline.GameDatabase;
    }

    protected override void DisposeManagedResources()
    {
        base.DisposeManagedResources();
        _primitiveXmlParserErrorProvider.XmlParseError -= OnXmlParseError;
        _primitiveXmlParserErrorProvider = null!;
    }

    public void OnXmlParseError(IPetroglyphXmlParser parser, XmlParseErrorEventArgs error)
    {
        XmlParseError?.Invoke(parser, error);
    }
}