using System;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.CommonUtilities;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.Database.Initialization;
using PG.StarWarsGame.Engine.Repositories;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Database;

internal class GameDatabaseService(IServiceProvider serviceProvider) : IGameDatabaseService
{
    public async Task<IGameDatabase> InitializeGameAsync(
        GameEngineType targetEngineType, 
        GameLocations locations,
        IDatabaseErrorListener? errorListener = null,
        CancellationToken cancellationToken = default)
    {
        var repoFactory = serviceProvider.GetRequiredService<IGameRepositoryFactory>();

        using var errorListenerWrapper = new DatabaseErrorListenerWrapper(errorListener, serviceProvider);
        var repository = repoFactory.Create(targetEngineType, locations, errorListenerWrapper);

        var pipeline = new GameDatabaseCreationPipeline(repository, errorListenerWrapper, serviceProvider);
        await pipeline.RunAsync(cancellationToken);
        return pipeline.GameDatabase;
    }
}

internal class DatabaseErrorListenerWrapper : DisposableObject, IDatabaseErrorListener, IXmlParserErrorListener
{
    private readonly IDatabaseErrorListener? _errorListener;
    private IPrimitiveXmlErrorParserProvider? _primitiveXmlParserErrorProvider;

    public DatabaseErrorListenerWrapper(IDatabaseErrorListener? errorListener, IServiceProvider serviceProvider)
    {
        _errorListener = errorListener;
        
        if (_errorListener is null)
            return;

        _primitiveXmlParserErrorProvider = serviceProvider.GetRequiredService<IPrimitiveXmlErrorParserProvider>();
        _primitiveXmlParserErrorProvider.XmlParseError += OnXmlParseError;

    }

    public void OnXmlParseError(IPetroglyphXmlParser parser, XmlParseErrorEventArgs error)
    {
        if (_errorListener is null)
            return;

        var location = error.Element is null ? default : XmlLocationInfo.FromElement(error.Element);
        OnXmlError(new XmlError
        {
            File = error.File,
            Parser = parser.ToString(), 
            Message = error.Message,
            ErrorKind = error.ErrorKind, 
            Location = location
        });
    }

    protected override void DisposeManagedResources()
    {
        base.DisposeManagedResources();
        if (_primitiveXmlParserErrorProvider is null)
            return;
        _primitiveXmlParserErrorProvider.XmlParseError -= OnXmlParseError;
        _primitiveXmlParserErrorProvider = null!;
    }

    public void OnXmlError(XmlError error)
    {
        _errorListener?.OnXmlError(error);
    }
}


public interface IDatabaseErrorListener
{
    void OnXmlError(XmlError error);
}

public abstract class DatabaseErrorListener : DisposableObject, IDatabaseErrorListener
{
    public virtual void OnXmlError(XmlError error)
    {
    }
}


public sealed class XmlError
{
    public string File { get; init; }

    public string Parser { get; init; }

    public XmlLocationInfo Location { get; init; }

    public XmlParseErrorKind ErrorKind { get; init; }

    public string Message { get; init; }
}