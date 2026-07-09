using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace AET.ModVerify.Reporting.Engine;

/// <summary>Collects the XML, initialization, and assertion errors reported by the game engine during verification.</summary>
public sealed class GameEngineErrorCollection : IGameEngineErrorCollection, IGameEngineErrorReporter
{
    private readonly ConcurrentBag<XmlError> _xmlErrors = [];
    private readonly ConcurrentBag<InitializationError> _initializationErrors = [];
    private readonly ConcurrentBag<EngineAssert> _asserts = [];

    /// <inheritdoc />
    public IEnumerable<XmlError> XmlErrors => _xmlErrors.ToList();

    /// <inheritdoc />
    public IEnumerable<InitializationError> InitializationErrors => _initializationErrors.ToList();

    /// <inheritdoc />
    public IEnumerable<EngineAssert> Asserts => _asserts.ToList();

    void IXmlParserErrorReporter.Report(XmlError error)
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