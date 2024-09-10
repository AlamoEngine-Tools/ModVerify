namespace PG.StarWarsGame.Files.XML.ErrorHandling;

public enum XmlParseErrorKind
{
    /// <summary>
    /// The error not specified any further.
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// The XML file could not be found.
    /// </summary>
    MissingFile = 1,
    /// <summary>
    /// The root node of an XML file is empty.
    /// </summary>
    EmptyRoot = 2,
    /// <summary>
    /// A tag's value is syntactically correct, but the semantics of value are not valid. For example,
    /// when the input is '-1' but an uint type is expected.
    /// </summary>
    InvalidValue = 3,
    /// <summary>
    /// A tag's value is has an invalid syntax.
    /// </summary>
    MalformedValue = 4,
    /// <summary>
    /// The value is too long
    /// </summary>
    TooLongData = 5,
    /// <summary>
    /// The data is missing an XML attribute. Usually this is the Name attribute.
    /// </summary>
    MissingAttribute = 6,
    /// <summary>
    /// The data points to a non-existing reference.
    /// </summary>
    MissingReference = 7,
    /// <summary>
    /// The XML file does not start with the XML header.
    /// </summary>
    DataBeforeHeader = 8,
    /// <summary>
    /// The XML file is missing an expected node.
    /// </summary>
    MissingNode = 9
}