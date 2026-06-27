using System.Collections.Generic;
using AET.ModVerify.Verifiers;

namespace AET.ModVerify.Reporting.Diagnostics;

/// <summary>Provides factories for findings produced while verifying models, particles, and animations.</summary>
public static class ModelErrors
{
    private static readonly ErrorDescriptor _alamoFileNotFound = new(
        VerifierErrorCodes.FileNotFound, "AlamoFileNotFound", VerificationSeverity.Error, "Models");

    private static readonly ErrorDescriptor _unexpectedAloType = new(
        VerifierErrorCodes.UnexpectedBinaryFormat, "UnexpectedAloType", VerificationSeverity.Error, "Models");

    private static readonly ErrorDescriptor _crcCollision = new(
        VerifierErrorCodes.UnexpectedFileLoad, "ModelCrcCollision", VerificationSeverity.Error, "Models");

    private static readonly ErrorDescriptor _animationCrcCollision = new(
        VerifierErrorCodes.UnexpectedFileLoad, "AnimationCrcCollision", VerificationSeverity.Information, "Models");

    private static readonly ErrorDescriptor _corruptModel = new(
        VerifierErrorCodes.BinaryFileCorrupt, "CorruptModel", VerificationSeverity.Critical, "Models");

    private static readonly ErrorDescriptor _corruptAnimation = new(
        VerifierErrorCodes.BinaryFileCorrupt, "CorruptAnimation", VerificationSeverity.Error, "Models");

    private static readonly ErrorDescriptor _invalidParticleTextureName = new(
        VerifierErrorCodes.InvalidFilePath, "InvalidParticleTextureName", VerificationSeverity.Error, "Models");

    private static readonly ErrorDescriptor _invalidModelTextureName = new(
        VerifierErrorCodes.InvalidFilePath, "InvalidModelTextureName", VerificationSeverity.Error, "Models");

    private static readonly ErrorDescriptor _invalidShaderName = new(
        VerifierErrorCodes.InvalidFilePath, "InvalidShaderName", VerificationSeverity.Error, "Models");

    private static readonly ErrorDescriptor _invalidProxyName = new(
        VerifierErrorCodes.InvalidFilePath, "InvalidProxyName", VerificationSeverity.Error, "Models");

    private static readonly ErrorDescriptor _particleNameMismatch = new(
        VerifierErrorCodes.InvalidParticleName, "ParticleNameMismatch", VerificationSeverity.Error, "Models");

    private static readonly ErrorDescriptor _emptyTextureName = new(
        VerifierErrorCodes.InvalidValue, "EmptyTextureName", VerificationSeverity.Error, "Models");

    private static readonly ErrorDescriptor _emptyProxyName = new(
        VerifierErrorCodes.InvalidValue, "EmptyProxyName", VerificationSeverity.Error, "Models");

    private static readonly ErrorDescriptor _proxyNotFound = new(
        VerifierErrorCodes.FileNotFound, "ProxyParticleNotFound", VerificationSeverity.Error, "Models");

    private static readonly ErrorDescriptor _shaderNotFound = new(
        VerifierErrorCodes.FileNotFound, "ShaderEffectNotFound", VerificationSeverity.Error, "Models");

    /// <summary>Creates an error for an Alamo file (model, particle, or animation) that could not be found.</summary>
    public static VerificationError AlamoFileNotFound(IGameVerifierInfo verifier, string fileName, IEnumerable<string> context)
        => _alamoFileNotFound.Create(verifier, $"Unable to find Alamo file '{fileName}'.", fileName, context);

    /// <summary>Creates an error for an Alamo file that is not of the expected renderable type.</summary>
    public static VerificationError UnexpectedAloType(IGameVerifierInfo verifier, string expectedType, string actualType, string asset, IEnumerable<string> context)
        => _unexpectedAloType.Create(verifier, $"Expected Alamo object of type {expectedType}, but got {actualType}.", asset, context);

    /// <summary>Creates an error for a model load that resolved a different file than requested (likely CRC32 collision).</summary>
    public static VerificationError CrcCollision(IGameVerifierInfo verifier, string requestedFile, string foundFile, IEnumerable<string> context)
        => _crcCollision.Create(verifier, $"Possible file CRC32 collision: '{requestedFile}' was requested but '{foundFile}' was found by the engine.", requestedFile, context);

    /// <summary>Creates an error for an animation load that resolved a different file than requested (likely CRC32 collision).</summary>
    public static VerificationError AnimationCrcCollision(IGameVerifierInfo verifier, string requestedFile, string foundFile, IEnumerable<string> context)
        => _animationCrcCollision.Create(verifier, $"Possible file CRC32 collision: '{requestedFile}' was requested but '{foundFile}' was found by the engine.", requestedFile, context);

    /// <summary>Creates an error for a corrupted model or particle file.</summary>
    public static VerificationError CorruptModel(IGameVerifierInfo verifier, string fileName, string detail, IEnumerable<string> context)
        => _corruptModel.Create(verifier, $"'{fileName}' is corrupted: {detail}", fileName, context);

    /// <summary>Creates an error for a corrupted animation file referenced by a model.</summary>
    public static VerificationError CorruptAnimation(IGameVerifierInfo verifier, string animationFile, string modelFile, IEnumerable<string> context)
        => _corruptAnimation.Create(verifier, $"Invalid animation file '{animationFile}' for model '{modelFile}'.", animationFile, context);

    /// <summary>Creates an error for an invalid texture file name in a particle.</summary>
    public static VerificationError InvalidParticleTextureName(IGameVerifierInfo verifier, string texture, string particleFile, IEnumerable<string> context)
        => _invalidParticleTextureName.Create(verifier, $"Invalid texture file name '{texture}' in particle '{particleFile}'.", texture, context);

    /// <summary>Creates an error for an invalid texture file name in a model.</summary>
    public static VerificationError InvalidModelTextureName(IGameVerifierInfo verifier, string texture, string modelFile, IEnumerable<string> context)
        => _invalidModelTextureName.Create(verifier, $"Invalid texture file name '{texture}' in model '{modelFile}'.", texture, context);

    /// <summary>Creates an error for an invalid shader file name in a model.</summary>
    public static VerificationError InvalidShaderName(IGameVerifierInfo verifier, string shader, string modelFile, IEnumerable<string> context)
        => _invalidShaderName.Create(verifier, $"Invalid shader file name '{shader}' in model '{modelFile}'.", shader, context);

    /// <summary>Creates an error for an invalid proxy file name in a model.</summary>
    public static VerificationError InvalidProxyName(IGameVerifierInfo verifier, string proxy, string modelFile, IEnumerable<string> context)
        => _invalidProxyName.Create(verifier, $"Invalid proxy file name '{proxy}' for model '{modelFile}'.", proxy, context);

    /// <summary>Creates an error for a particle whose internal name does not match its file name.</summary>
    public static VerificationError ParticleNameMismatch(IGameVerifierInfo verifier, string particleName, string fileName, IEnumerable<string> context)
        => _particleNameMismatch.Create(verifier, $"The particle name '{particleName}' does not match file name '{fileName}'.", particleName, context);

    /// <summary>Creates an error for an empty texture reference in a model or particle.</summary>
    public static VerificationError EmptyTextureName(IGameVerifierInfo verifier, string modelOrParticleFile, IEnumerable<string> context)
        => _emptyTextureName.Create(verifier, $"Texture string in model or particle '{modelOrParticleFile}' is empty.", modelOrParticleFile, context);

    /// <summary>Creates an error for an empty proxy name in a model.</summary>
    public static VerificationError EmptyProxyName(IGameVerifierInfo verifier, string modelFile, IEnumerable<string> context)
        => _emptyProxyName.Create(verifier, $"Proxy name in model '{modelFile}' is empty.", modelFile, context);

    /// <summary>Creates an error for a proxy particle that could not be found for a model.</summary>
    public static VerificationError ProxyNotFound(IGameVerifierInfo verifier, string proxyName, string modelFile, IEnumerable<string> context)
        => _proxyNotFound.Create(verifier, $"Proxy particle '{proxyName}' not found for model '{modelFile}'.", proxyName, context);

    /// <summary>Creates an error for a shader effect that could not be found for a model.</summary>
    public static VerificationError ShaderNotFound(IGameVerifierInfo verifier, string shaderEffect, string modelFile, IEnumerable<string> context)
        => _shaderNotFound.Create(verifier, $"Shader effect '{shaderEffect}' not found for model '{modelFile}'.", shaderEffect, context);
}
