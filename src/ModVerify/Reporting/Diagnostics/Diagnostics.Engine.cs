using System.Collections.Generic;
using AET.ModVerify.Verifiers;

namespace AET.ModVerify.Reporting.Diagnostics;

/// <summary>Provides the engine-initialization descriptor read by the engine reporter.</summary>
public static class EngineErrors
{
    /// <summary>The game engine reported an error during initialization. The message is supplied by the engine.</summary>
    public static readonly ErrorDescriptor InitializationError = new(
        VerifierErrorCodes.InitializationError, "EngineInitializationError", VerificationSeverity.Critical, "Engine");
}

/// <summary>Provides factories for hard-coded engine assets that are loaded at startup.</summary>
public static class HardcodedAssetErrors
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

/// <summary>Provides descriptors for assertions raised by the engine.</summary>
public static class AssertErrors
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
