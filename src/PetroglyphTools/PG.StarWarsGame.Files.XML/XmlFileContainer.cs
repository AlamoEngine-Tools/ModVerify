using System.Collections.Generic;

namespace PG.StarWarsGame.Files.XML;

public class XmlFileContainer(IList<string> files)
{
    public IList<string> Files { get; } = files;
}