using System.Collections.Generic;

namespace PG.StarWarsGame.Files.XML.Data;

public class XmlFileContainer(IList<string> files)
{
    public IList<string> Files { get; } = files;
}