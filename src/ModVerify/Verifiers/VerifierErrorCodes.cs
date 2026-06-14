namespace AET.ModVerify.Verifiers;

/// <summary>
/// Provides the error codes used by the verifiers in the ModVerify framework.
/// </summary>
public static class VerifierErrorCodes
{
    /// <summary>
    /// The error code for initialization errors that occur on critical failures when a game manager is initializing.
    /// </summary>
    public const string InitializationError = "INIT00";

    /// <summary>
    /// The error code for an engine assertion that occur when a value is null or empty.
    /// </summary>
    public const string AssertValueNullOrEmpty = "ASRT00";
    /// <summary>
    /// The error code for an engine assertion that occur when a value is out of the expected range.
    /// </summary>
    public const string AssertValueOutOfRange = "ASRT01";
    /// <summary>
    /// The error code for an engine assertion that occur when a value is invalid or malformed.
    /// </summary>
    public const string AssertValueInvalid = "ASRT02";

    /// <summary>
    /// The error code for a binary file that is corrupt or cannot be parsed.
    /// </summary>
    public const string BinaryFileCorrupt = "BIN00";
    /// <summary>
    /// The error code for a binary file that has an unexpected format or structure.
    /// </summary>
    public const string UnexpectedBinaryFormat = "BIN01";
    /// <summary>
    /// The error code for a binary file that contains an invalid or unsupported value.
    /// </summary>
    public const string InvalidValue = "BIN02";

    public const string FileNotFound = "FILE00";
    public const string FilePathTooLong = "FILE01";
    public const string InvalidFilePath = "FILE02";
    public const string UnexpectedFileLoad = "FILE03";

    public const string Duplicate = "DUP00";
    public const string MissingXRef = "XREF00";

    public const string NameTooLong = "NAME00";

    public const string UnrecognizedEnum = "ENUM00";

    public const string SampleNotPCM = "WAV00";
    public const string SampleNotMono = "WAV01";
    public const string InvalidSampleRate = "WAV02";
    public const string InvalidBitsPerSeconds = "WAV03";

    public const string InvalidParticleName = "ALO01";

    public const string GenericXmlError = "XML00";
    public const string EmptyXmlRoot = "XML01";
    public const string InvalidXmlValue = "XML03";
    public const string MalformedXmlValue = "XML04";
    public const string MissingXmlAttribute = "XML05";
    public const string MissingXmlReference = "XML06";
    public const string XmlValueTooLong = "XML07";
    public const string XmlDataBeforeHeader = "XML08";
    public const string XmlMissingNode = "XML09";
    public const string XmlUnsupportedTag = "XML10";
    public const string XmlElementsInTag = "XML11";
    public const string XmlUnexceptedElementName = "XML12";
    public const string XmlEmptyNodeName = "XML13";

    public const string CommandBarNoShellsGroup = "CMDBAR00";
    public const string CommandBarManyShellsGroup = "CMDBAR01";
    public const string CommandBarNoShellsComponentInShellGroup = "CMDBAR02";
    public const string CommandBarUnsupportedComponent = "CMDBAR03";
    public const string CommandBarShellNoModel = "CMDBAR04";
    public const string CommandBarComponentNotConnected = "CMDBAR05";
}