using System.Collections.Generic;
using AET.ModVerify.Verifiers;

namespace AET.ModVerify.Reporting.Diagnostics;

/// <summary>Provides the engine-initialization descriptor read by the engine reporter.</summary>
public static class Engine
{
    /// <summary>The game engine reported an error during initialization. The message is supplied by the engine.</summary>
    public static readonly ErrorDescriptor InitializationError = new(
        VerifierErrorCodes.InitializationError, "EngineInitializationError", VerificationSeverity.Critical, "Engine");
}

/// <summary>Provides factories for hard-coded engine assets that are loaded at startup.</summary>
public static class HardcodedAssets
{
    private static readonly ErrorDescriptor _shaderNotFound = new(
        VerifierErrorCodes.FileNotFound, "HardcodedShaderNotFound", VerificationSeverity.Error, "HardcodedAssets");

    private static readonly ErrorDescriptor _terrainShaderNotFound = new(
        VerifierErrorCodes.FileNotFound, "HardcodedTerrainShaderNotFound", VerificationSeverity.Error, "HardcodedAssets");

    /// <summary>Creates an error for a shader the engine loads at startup that could not be found.</summary>
    /// <returns>A new error for the finding.</returns>
    public static VerificationError ShaderNotFound(IGameVerifierInfo verifier, string shaderName, IEnumerable<string> context)
        => _shaderNotFound.Create(verifier, $"Unable to find shader '{shaderName}'.", shaderName, context);

    /// <summary>Creates an error for a terrain shader the engine loads on terrain load that could not be found.</summary>
    /// <returns>A new error for the finding.</returns>
    public static VerificationError TerrainShaderNotFound(IGameVerifierInfo verifier, string shaderName, IEnumerable<string> context)
        => _terrainShaderNotFound.Create(verifier, $"Unable to find terrain shader '{shaderName}'.", shaderName, context);
}

// Descriptors for findings produced by the engine-side reporters. Their messages are supplied by the engine
// (XML parser, asserts), so the reporters read only Id and Severity.

/// <summary>Provides descriptors for findings reported by the engine's XML parser.</summary>
public static class Xml
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

/// <summary>Provides descriptors for assertions raised by the engine.</summary>
public static class Asserts
{
    /// <summary>The engine asserted that a value was null or empty.</summary>
    public static readonly ErrorDescriptor NullOrEmptyValue = new(
        VerifierErrorCodes.AssertValueNullOrEmpty, "AssertNullOrEmptyValue", VerificationSeverity.Warning, "Asserts");

    /// <summary>The engine asserted that a value was out of range.</summary>
    public static readonly ErrorDescriptor ValueOutOfRange = new(
        VerifierErrorCodes.AssertValueOutOfRange, "AssertValueOutOfRange", VerificationSeverity.Warning, "Asserts");

    /// <summary>The engine asserted that a value was invalid.</summary>
    public static readonly ErrorDescriptor InvalidValue = new(
        VerifierErrorCodes.AssertValueInvalid, "AssertInvalidValue", VerificationSeverity.Warning, "Asserts");

    /// <summary>The engine asserted that a file was not found.</summary>
    public static readonly ErrorDescriptor FileNotFound = new(
        VerifierErrorCodes.FileNotFound, "AssertFileNotFound", VerificationSeverity.Warning, "Asserts");

    /// <summary>The engine asserted that a duplicate entry exists.</summary>
    public static readonly ErrorDescriptor DuplicateEntry = new(
        VerifierErrorCodes.Duplicate, "AssertDuplicateEntry", VerificationSeverity.Warning, "Asserts");

    /// <summary>The engine asserted that a binary file is corrupt.</summary>
    public static readonly ErrorDescriptor CorruptBinary = new(
        VerifierErrorCodes.BinaryFileCorrupt, "AssertCorruptBinary", VerificationSeverity.Warning, "Asserts");
}
