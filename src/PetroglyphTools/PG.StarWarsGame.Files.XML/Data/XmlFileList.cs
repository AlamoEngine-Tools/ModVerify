using System.Collections.Generic;

namespace PG.StarWarsGame.Files.XML.Data;

public class XmlFileList(IReadOnlyList<string> files, XmlLocationInfo location) : XmlObject(location)
{
    public static XmlFileList Empty(XmlLocationInfo location)
    {
        return new XmlFileList([], location);
    }

    public IReadOnlyList<string> Files { get; } = files;
}