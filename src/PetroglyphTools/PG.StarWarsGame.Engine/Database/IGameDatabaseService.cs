using System;
using System.Threading;
using System.Threading.Tasks;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Engine.Database;

public interface IGameDatabaseService : IXmlParserErrorProvider, IDisposable
{
    Task<IGameDatabase> CreateDatabaseAsync(
        GameEngineType targetEngineType, 
        GameLocations locations,
        CancellationToken cancellationToken = default);
}