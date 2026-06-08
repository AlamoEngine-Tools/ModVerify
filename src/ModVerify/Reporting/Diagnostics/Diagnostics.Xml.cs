using AET.ModVerify.Verifiers;

namespace AET.ModVerify.Reporting.Diagnostics;

/// <summary>Provides descriptors for findings reported by the engine's XML parser.</summary>
public static class XmlErrors
{
    /// <summary>The XML parser reported an error that does not map to a more specific kind.</summary>
    public static readonly ErrorDescriptor Generic = new(
        VerifierErrorCodes.GenericXmlError, "XmlGenericError", VerificationSeverity.Warning, "Xml");

    /// <summary>An XML file has an empty root element.</summary>
    public static readonly ErrorDescriptor EmptyRoot = new(
        VerifierErrorCodes.EmptyXmlRoot, "XmlEmptyRoot", VerificationSeverity.Critical, "Xml");

    /// <summary>An XML file referenced by the parser could not be found.</summary>
    public static readonly ErrorDescriptor MissingFile = new(
        VerifierErrorCodes.FileNotFound, "XmlMissingFile", VerificationSeverity.Error, "Xml");

    /// <summary>An XML element has an invalid value.</summary>
    public static readonly ErrorDescriptor InvalidValue = new(
        VerifierErrorCodes.InvalidXmlValue, "XmlInvalidValue", VerificationSeverity.Information, "Xml");

    /// <summary>An XML element has a malformed value.</summary>
    public static readonly ErrorDescriptor MalformedValue = new(
        VerifierErrorCodes.MalformedXmlValue, "XmlMalformedValue", VerificationSeverity.Warning, "Xml");

    /// <summary>An XML element is missing a required attribute.</summary>
    public static readonly ErrorDescriptor MissingAttribute = new(
        VerifierErrorCodes.MissingXmlAttribute, "XmlMissingAttribute", VerificationSeverity.Error, "Xml");

    /// <summary>An XML element references an entry that does not exist.</summary>
    public static readonly ErrorDescriptor MissingReference = new(
        VerifierErrorCodes.MissingXmlReference, "XmlMissingReference", VerificationSeverity.Error, "Xml");

    /// <summary>An XML value exceeds the maximum allowed length.</summary>
    public static readonly ErrorDescriptor ValueTooLong = new(
        VerifierErrorCodes.XmlValueTooLong, "XmlValueTooLong", VerificationSeverity.Warning, "Xml");

    /// <summary>An XML file contains data before its header.</summary>
    public static readonly ErrorDescriptor DataBeforeHeader = new(
        VerifierErrorCodes.XmlDataBeforeHeader, "XmlDataBeforeHeader", VerificationSeverity.Information, "Xml");

    /// <summary>A required XML node is missing.</summary>
    public static readonly ErrorDescriptor MissingNode = new(
        VerifierErrorCodes.XmlMissingNode, "XmlMissingNode", VerificationSeverity.Critical, "Xml");

    /// <summary>An XML file contains a node the parser does not support.</summary>
    public static readonly ErrorDescriptor UnknownNode = new(
        VerifierErrorCodes.XmlUnsupportedTag, "XmlUnknownNode", VerificationSeverity.Information, "Xml");

    /// <summary>An XML tag unexpectedly contains child elements.</summary>
    public static readonly ErrorDescriptor TagHasElements = new(
        VerifierErrorCodes.XmlElementsInTag, "XmlTagHasElements", VerificationSeverity.Warning, "Xml");

    /// <summary>An XML element has a name the parser did not expect.</summary>
    public static readonly ErrorDescriptor UnexpectedElementName = new(
        VerifierErrorCodes.XmlUnexceptedElementName, "XmlUnexpectedElementName", VerificationSeverity.Information, "Xml");

    /// <summary>An XML node has an empty name.</summary>
    public static readonly ErrorDescriptor EmptyNodeName = new(
        VerifierErrorCodes.XmlEmptyNodeName, "XmlEmptyNodeName", VerificationSeverity.Warning, "Xml");
}
