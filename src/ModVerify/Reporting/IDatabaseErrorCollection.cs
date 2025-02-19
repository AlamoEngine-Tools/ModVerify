using System.Collections.Generic;
using PG.StarWarsGame.Engine.ErrorReporting;

namespace AET.ModVerify.Reporting;

internal interface IDatabaseErrorCollection
{
    IEnumerable<XmlError> XmlErrors { get; }
    IEnumerable<InitializationError> InitializationErrors { get; }
}