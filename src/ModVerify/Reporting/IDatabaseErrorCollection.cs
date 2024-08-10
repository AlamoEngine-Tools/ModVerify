using System.Collections.Generic;
using PG.StarWarsGame.Engine.Database.ErrorReporting;

namespace AET.ModVerify.Reporting;

internal interface IDatabaseErrorCollection
{
    public IEnumerable<XmlError> XmlErrors { get; }
}