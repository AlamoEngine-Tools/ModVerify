using System.Collections.Generic;

namespace PG.StarWarsGame.Files.XML.Data;

public class XmlFileList(IReadOnlyList<string> files)
{
    public static readonly XmlFileList Empty = new([]);

    public IReadOnlyList<string> Files { get; } = files;
}