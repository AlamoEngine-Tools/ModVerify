using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PG.StarWarsGame.Engine.ErrorReporting;

namespace AET.ModVerify.Reporting;

internal class ConcurrentGameGameErrorReporter : GameErrorReporter, IDatabaseErrorCollection
{
    private readonly ConcurrentBag<XmlError> _xmlErrors = new();
    private readonly ConcurrentBag<InitializationError> _initializationErrors = new();
    private readonly ConcurrentBag<EngineAssert> _asserts = new();

    public IEnumerable<XmlError> XmlErrors => _xmlErrors.ToList();

    public IEnumerable<InitializationError> InitializationErrors => _initializationErrors.ToList();

    public IEnumerable<EngineAssert> Asserts => _asserts.ToList();

    public override void Report(XmlError error)
    {
        _xmlErrors.Add(error);
    }

    public override void Report(InitializationError error)
    {
        _initializationErrors.Add(error);
    }

    public override void Assert(EngineAssert assert)
    {
        _asserts.Add(assert);
    }
}