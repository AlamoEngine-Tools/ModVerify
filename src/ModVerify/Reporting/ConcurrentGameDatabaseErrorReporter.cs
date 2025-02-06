using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PG.StarWarsGame.Engine.Database.ErrorReporting;

namespace AET.ModVerify.Reporting;

internal class ConcurrentGameDatabaseErrorReporter : DatabaseErrorReporter, IDatabaseErrorCollection
{
    private readonly ConcurrentBag<XmlError> _xmlErrors = new();

    private readonly ConcurrentBag<InitializationError> _initializationErrors = new();

    public IEnumerable<XmlError> XmlErrors => _xmlErrors.ToList();
    public IEnumerable<InitializationError> InitializationErrors => _initializationErrors.ToList();

    public override void Report(XmlError error)
    {
        _xmlErrors.Add(error);
    }

    public override void Report(InitializationError error)
    {
        _initializationErrors.Add(error);
    }
}