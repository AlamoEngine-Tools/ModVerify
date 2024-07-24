using System;
using System.Xml.Linq;
using AnakinRaW.CommonUtilities;

namespace PG.StarWarsGame.Files.XML.ErrorHandling;

public class XmlParseErrorEventArgs : EventArgs
{
    public string File { get; }

    public XElement? Element { get; }

    public XmlParseErrorKind ErrorKind { get; }

    public string Message { get; }

    public XmlParseErrorEventArgs(string file, XElement? element, XmlParseErrorKind errorKind, string message)
    {
        ThrowHelper.ThrowIfNullOrEmpty(file);
        File = file;
        Element = element;
        ErrorKind = errorKind;
        Message = message;
    }

    public static XmlParseErrorEventArgs FromMissingFile(string file)
    {
        ThrowHelper.ThrowIfNullOrEmpty(file);
        return new XmlParseErrorEventArgs(file, null, XmlParseErrorKind.MissingFile, $"XML file '{file}' not found.");
    }

    public static XmlParseErrorEventArgs FromEmptyRoot(string file, XElement element)
    {
        ThrowHelper.ThrowIfNullOrEmpty(file);
        return new XmlParseErrorEventArgs(file, element, XmlParseErrorKind.EmptyRoot, $"XML file '{file}' has an empty root node.");
    }
}