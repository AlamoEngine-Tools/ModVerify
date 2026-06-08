using System.Collections.Generic;
using AET.ModVerify.Verifiers;

namespace AET.ModVerify.Reporting.Diagnostics;

/// <summary>Provides factories for findings produced while verifying game object types.</summary>
public static class GameObjectErrors
{
    private static readonly ErrorDescriptor _nameTooLong = new(
        VerifierErrorCodes.NameTooLong, "GameObjectNameTooLong", VerificationSeverity.Critical, "GameObjects");

    private static readonly ErrorDescriptor _missingBaseType = new(
        VerifierErrorCodes.MissingXRef, "MissingBaseType", VerificationSeverity.Critical, "GameObjects");

    private static readonly ErrorDescriptor _missingCompanyUnit = new(
        VerifierErrorCodes.MissingXRef, "MissingCompanyUnit", VerificationSeverity.Critical, "GameObjects");

    private static readonly ErrorDescriptor _expectedModelOrParticle = new(
        VerifierErrorCodes.UnexpectedBinaryFormat, "ExpectedModelOrParticle", VerificationSeverity.Error, "GameObjects");

    private static readonly ErrorDescriptor _duplicateTerrainMapping = new(
        VerifierErrorCodes.Duplicate, "DuplicateTerrainMapping", VerificationSeverity.Warning, "GameObjects");

    private static readonly ErrorDescriptor _invalidTerrainType = new(
        VerifierErrorCodes.UnrecognizedEnum, "InvalidTerrainType", VerificationSeverity.Error, "GameObjects");

    private static readonly ErrorDescriptor _iconNotFound = new(
        VerifierErrorCodes.FileNotFound, "GameObjectIconNotFound", VerificationSeverity.Warning, "GameObjects");

    /// <summary>Creates an error for a game object type name that exceeds the maximum length.</summary>
    public static VerificationError NameTooLong(IGameVerifierInfo verifier, string name, int maxLength, IEnumerable<string> context)
        => _nameTooLong.Create(verifier, $"The GameObjectType name '{name}' is too long. Maximum length is {maxLength}.", name, context);

    /// <summary>Creates an error for a game object type that inherits from a missing base type.</summary>
    public static VerificationError MissingBaseType(IGameVerifierInfo verifier, string baseType, string gameObjectName, IEnumerable<string> context)
        => _missingBaseType.Create(verifier, $"Missing base type '{baseType}' for GameObject '{gameObjectName}'.", baseType, context);

    /// <summary>Creates an error for a game object type that references a missing company unit.</summary>
    public static VerificationError MissingCompanyUnit(IGameVerifierInfo verifier, string companyUnit, string gameObjectName, IEnumerable<string> context)
        => _missingCompanyUnit.Create(verifier, $"Missing company unit '{companyUnit}' for GameObject '{gameObjectName}'.", companyUnit, context);

    /// <summary>Creates an error for a model slot that expects a model or particle but was given an animation.</summary>
    public static VerificationError ExpectedModelOrParticle(IGameVerifierInfo verifier, string modelKind, string gameObjectName, string asset, IEnumerable<string> context)
        => _expectedModelOrParticle.Create(verifier, $"Expected Model or Particle as {modelKind} for '{gameObjectName}', but found an animation.", asset, context);

    /// <summary>Creates an error for a terrain type mapped to a land model override more than once.</summary>
    public static VerificationError DuplicateTerrainMapping(IGameVerifierInfo verifier, string terrainType, string gameObjectName, IEnumerable<string> context)
        => _duplicateTerrainMapping.Create(verifier, $"Terrain type '{terrainType}' for land model override is defined multiple times for game object type {gameObjectName}.", terrainType, context);

    /// <summary>Creates an error for a land model override that specifies an unrecognized terrain type.</summary>
    public static VerificationError InvalidTerrainType(IGameVerifierInfo verifier, string terrainType, string tag, string gameObjectName, IEnumerable<string> context)
        => _invalidTerrainType.Create(verifier, $"Invalid terrain type '{terrainType}' specified in {tag} for GameObjectType '{gameObjectName}'.", terrainType, context);

    /// <summary>Creates an error for a game object type icon that could not be found.</summary>
    public static VerificationError IconNotFound(IGameVerifierInfo verifier, string iconName, string gameObjectName, IEnumerable<string> context)
        => _iconNotFound.Create(verifier, $"Could not find icon '{iconName}' for game object type '{gameObjectName}'.", iconName, context);
}
