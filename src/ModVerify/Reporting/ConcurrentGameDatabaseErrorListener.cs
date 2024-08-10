using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PG.StarWarsGame.Engine.Database.ErrorReporting;

namespace AET.ModVerify.Reporting;

internal class ConcurrentGameDatabaseErrorListener : DatabaseErrorListener, IDatabaseErrorCollection
{
    private readonly ConcurrentBag<XmlError> _xmlErrors = new();

    public IEnumerable<XmlError> XmlErrors => _xmlErrors.ToList();

    public override void OnXmlError(XmlError error)
    {
        _xmlErrors.Add(error);
    }
}