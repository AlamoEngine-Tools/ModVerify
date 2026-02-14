using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PG.StarWarsGame.Engine.ErrorReporting;

namespace AET.ModVerify.Reporting.Engine;

public sealed class GameEngineErrorCollection : IGameEngineErrorCollection, IGameEngineErrorReporter
{
    private readonly ConcurrentBag<XmlError> _xmlErrors = new();
    private readonly ConcurrentBag<InitializationError> _initializationErrors = new();
    private readonly ConcurrentBag<EngineAssert> _asserts = new();

    public IEnumerable<XmlError> XmlErrors => _xmlErrors.ToList();

    public IEnumerable<InitializationError> InitializationErrors => _initializationErrors.ToList();

    public IEnumerable<EngineAssert> Asserts => _asserts.ToList();

    void IGameEngineErrorReporter.Report(XmlError error)
    {
        _xmlErrors.Add(error);
    }

    void IGameEngineErrorReporter.Report(InitializationError error)
    {
        _initializationErrors.Add(error);
    }

    void IGameEngineErrorReporter.Assert(EngineAssert assert)
    {
        _asserts.Add(assert);
    }

    internal void Clear()
    {
#if !NETFRAMEWORK && !NETSTANDARD2_0
        _xmlErrors.Clear();
        _initializationErrors.Clear();
        _asserts.Clear();
#else
        ClearUnsafe(_xmlErrors);
        ClearUnsafe(_initializationErrors);
        ClearUnsafe(_asserts);

        static void ClearUnsafe<T>(ConcurrentBag<T> bag)
        {
            while (bag.TryTake(out _)) ;
        }
#endif
    }
}