using System.Xml;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML;

public readonly struct XmlLocationInfo(string xmlFile, int? line)
{
    public bool HasLocation => !string.IsNullOrEmpty(XmlFile) && Line is not null;

    public string? XmlFile { get; } = xmlFile;

    public int? Line { get; } = line;


    public static XmlLocationInfo FromElement(XElement element)
    {
        if (element.Document is null)
            return default;
        if (element is IXmlLineInfo lineInfoHolder && lineInfoHolder.HasLineInfo())
            return new XmlLocationInfo(element.Document.BaseUri, lineInfoHolder.LineNumber);
        return new XmlLocationInfo(element.Document.BaseUri, null);
    }


    public override string ToString()
    {
        if (string.IsNullOrEmpty(XmlFile))
            return "No File information";
        return Line is null ? XmlFile! : $"{XmlFile} at line: {Line}";
    }
}