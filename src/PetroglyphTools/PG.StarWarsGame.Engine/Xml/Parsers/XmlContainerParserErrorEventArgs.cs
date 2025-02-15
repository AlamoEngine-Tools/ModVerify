using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace PG.StarWarsGame.Engine.Xml.Parsers;

internal class XmlContainerParserErrorEventArgs(string file, XmlException? exception = null, bool isXmlFileList = false)
{
    public bool Continue
    {
        get;
        // Once this is set to false, there is no way back.
        set => field &= value;
    } = true;

    public bool ErrorInXmlFileList { get; } = isXmlFileList;

    public string File { get; } = file;

    [MemberNotNullWhen(true, nameof(Exception))]
    public bool HasException => Exception is not null;

    public bool IsFileNotFound => !HasException;

    public XmlException? Exception { get; } = exception;
}