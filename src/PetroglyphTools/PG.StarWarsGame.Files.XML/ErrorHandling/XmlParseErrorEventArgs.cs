using System;
using System.Xml.Linq;
using AnakinRaW.CommonUtilities;

namespace PG.StarWarsGame.Files.XML.ErrorHandling;

public class XmlParseErrorEventArgs : EventArgs
{
    public XmlLocationInfo Location { get; }

    public XElement? Element { get; }

    public XmlParseErrorKind ErrorKind { get; }

    public string Message { get; }

    public XmlParseErrorEventArgs(XElement element, XmlParseErrorKind errorKind, string message)
    {
        Element = element ?? throw new ArgumentNullException(nameof(element));
        Location = XmlLocationInfo.FromElement(element);
        ErrorKind = errorKind;
        Message = message;
    }

    public XmlParseErrorEventArgs(XmlLocationInfo location, XmlParseErrorKind errorKind, string message)
    {
        Location = location;
        Message = message;
        ErrorKind = errorKind;
    }

    public static XmlParseErrorEventArgs FromMissingFile(string file)
    {
        ThrowHelper.ThrowIfNullOrEmpty(file);
        return new XmlParseErrorEventArgs(new XmlLocationInfo(file, null), XmlParseErrorKind.MissingFile, "XML file not found.");
    }

    public static XmlParseErrorEventArgs FromEmptyRoot(XElement element)
    {
        return new XmlParseErrorEventArgs(element, XmlParseErrorKind.EmptyRoot, "XML file has an empty root node.");
    }
}