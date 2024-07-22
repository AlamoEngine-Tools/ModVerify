using System;
using System.Xml.Linq;
using AnakinRaW.CommonUtilities;

namespace PG.StarWarsGame.Files.XML;

public class ParseErrorEventArgs : EventArgs
{
    public string File { get; }

    public XElement? Element { get; }

    public XmlParseErrorKind ErrorKind { get; }

    public string Message { get; }

    public ParseErrorEventArgs(string file, XElement? element, XmlParseErrorKind errorKind, string message)
    {
        ThrowHelper.ThrowIfNullOrEmpty(file);
        File = file;
        Element = element;
        ErrorKind = errorKind;
        Message = message;
    }

    public static ParseErrorEventArgs FromMissingFile(string file)
    {
        ThrowHelper.ThrowIfNullOrEmpty(file);
        return new ParseErrorEventArgs(file, null, XmlParseErrorKind.MissingFile, $"XML file '{file}' not found.");
    }

    public static ParseErrorEventArgs FromEmptyRoot(string file, XElement element)
    {
        ThrowHelper.ThrowIfNullOrEmpty(file);
        return new ParseErrorEventArgs(file, element, XmlParseErrorKind.EmptyRoot, $"XML file '{file}' has an empty root node.");
    }
}

public enum XmlParseErrorKind
{
    /// <summary>
    /// The error not specified any further.
    /// </summary>
    Unknown,
    /// <summary>
    /// The XML file could not be found.
    /// </summary>
    MissingFile,
    /// <summary>
    /// The root node of an XML file is empty.
    /// </summary>
    EmptyRoot,
    /// <summary>
    /// A tag's value is syntactically correct, but the semantics of value are not valid. For example,
    /// when the input is '-1' but an uint type is expected.
    /// </summary>
    InvalidValue,
    /// <summary>
    /// A tag's value is has an invalid syntax.
    /// </summary>
    MalformedValue,
    /// <summary>
    /// The value is too long
    /// </summary>
    TooLongData,
    /// <summary>
    /// The data is missing an XML attribute. Usually this is the Name attribute.
    /// </summary>
    MissingAttribute,
    /// <summary>
    /// The data points to a non-existing reference.
    /// </summary>
    MissingReference,
}