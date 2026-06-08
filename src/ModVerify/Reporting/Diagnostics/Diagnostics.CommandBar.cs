using System.Collections.Generic;
using AET.ModVerify.Verifiers;

namespace AET.ModVerify.Reporting.Diagnostics;

/// <summary>Provides factories for findings produced while verifying the command bar.</summary>
public static class CommandBarErrors
{
    private const string GameCommandBarAsset = "GameCommandBar";

    private static readonly ErrorDescriptor _unsupportedComponent = new(
        VerifierErrorCodes.CommandBarUnsupportedComponent, "CommandBarUnsupportedComponent", VerificationSeverity.Information, "CommandBar");

    private static readonly ErrorDescriptor _duplicateComponent = new(
        VerifierErrorCodes.Duplicate, "CommandBarDuplicateComponent", VerificationSeverity.Warning, "CommandBar");

    private static readonly ErrorDescriptor _componentNameTooLong = new(
        VerifierErrorCodes.NameTooLong, "CommandBarComponentNameTooLong", VerificationSeverity.Critical, "CommandBar");

    private static readonly ErrorDescriptor _shellNoModel = new(
        VerifierErrorCodes.CommandBarShellNoModel, "CommandBarShellNoModel", VerificationSeverity.Error, "CommandBar");

    private static readonly ErrorDescriptor _componentNotConnected = new(
        VerifierErrorCodes.CommandBarComponentNotConnected, "CommandBarComponentNotConnected", VerificationSeverity.Warning, "CommandBar");

    private static readonly ErrorDescriptor _noShellsGroup = new(
        VerifierErrorCodes.CommandBarNoShellsGroup, "CommandBarNoShellsGroup", VerificationSeverity.Error, "CommandBar");

    private static readonly ErrorDescriptor _manyShellsGroups = new(
        VerifierErrorCodes.CommandBarManyShellsGroup, "CommandBarManyShellsGroups", VerificationSeverity.Warning, "CommandBar");

    private static readonly ErrorDescriptor _nonShellInShellGroup = new(
        VerifierErrorCodes.CommandBarNoShellsComponentInShellGroup, "CommandBarNonShellInShellGroup", VerificationSeverity.Warning, "CommandBar");

    private static readonly ErrorDescriptor _megaTextureDirectoryNotFound = new(
        VerifierErrorCodes.FileNotFound, "CommandBarMegaTextureDirectoryNotFound", VerificationSeverity.Critical, "CommandBar");

    private static readonly ErrorDescriptor _megaTextureNotFound = new(
        VerifierErrorCodes.FileNotFound, "CommandBarMegaTextureNotFound", VerificationSeverity.Critical, "CommandBar");

    /// <summary>Creates an error for a command bar component the game does not support.</summary>
    public static VerificationError UnsupportedComponent(IGameVerifierInfo verifier, string componentName, IEnumerable<string> context)
        => _unsupportedComponent.Create(verifier, $"The CommandBar component '{componentName}' is not supported by the game.", componentName, context);

    /// <summary>Creates an error for two command bar components that share the same identifier.</summary>
    public static VerificationError DuplicateComponent(IGameVerifierInfo verifier, string componentName, object componentId, IEnumerable<string> context)
        => _duplicateComponent.Create(verifier, $"The CommandBar component '{componentName}' with ID '{componentId}' already exists.", componentName, context);

    /// <summary>Creates an error for a command bar shell component name that exceeds the maximum length.</summary>
    public static VerificationError ComponentNameTooLong(IGameVerifierInfo verifier, string componentName, int maxLength, IEnumerable<string> context)
        => _componentNameTooLong.Create(verifier, $"The CommandBarShellComponent name '{componentName}' is too long. Maximum length is {maxLength}.", componentName, context);

    /// <summary>Creates an error for a command bar shell component that has no model specified.</summary>
    public static VerificationError ShellNoModel(IGameVerifierInfo verifier, string componentName, IEnumerable<string> context)
        => _shellNoModel.Create(verifier, $"The CommandBarShellComponent '{componentName}' has no model specified.", componentName, context);

    /// <summary>Creates an error for a command bar component that is not connected to a shell component.</summary>
    public static VerificationError ComponentNotConnected(IGameVerifierInfo verifier, string componentName, IEnumerable<string> context)
        => _componentNotConnected.Create(verifier, $"The CommandBar component '{componentName}' is not connected to a shell component.", componentName, context);

    /// <summary>Creates an error for the missing required shells command bar group.</summary>
    public static VerificationError NoShellsGroup(IGameVerifierInfo verifier, string groupName, IEnumerable<string> context)
        => _noShellsGroup.Create(verifier, $"No CommandBarGroup '{groupName}' found.", GameCommandBarAsset, context);

    /// <summary>Creates an error for more than one shells command bar group being defined.</summary>
    public static VerificationError ManyShellsGroups(IGameVerifierInfo verifier, string groupName, IEnumerable<string> context)
        => _manyShellsGroups.Create(verifier, $"Found more than one Shells CommandBarGroup. Mind that group names are case-sensitive. Correct name is '{groupName}'.", GameCommandBarAsset, context);

    /// <summary>Creates an error for a non-shell component that is a member of the shells command bar group.</summary>
    public static VerificationError NonShellInShellGroup(IGameVerifierInfo verifier, string componentName, string groupName, IEnumerable<string> context)
        => _nonShellInShellGroup.Create(verifier, $"The CommandBar component '{componentName}' is not a shell component, but part of the '{groupName}' group.", componentName, context);

    /// <summary>Creates an error for the command bar mega-texture directory that could not be found.</summary>
    public static VerificationError MegaTextureDirectoryNotFound(IGameVerifierInfo verifier, string baseName, IEnumerable<string> context)
        => _megaTextureDirectoryNotFound.Create(verifier, $"Cannot find CommandBar MegaTextureDirectory '{baseName}.mtd'.", $"{baseName}.mtd", context);

    /// <summary>Creates an error for the command bar mega-texture that could not be found.</summary>
    public static VerificationError MegaTextureNotFound(IGameVerifierInfo verifier, string baseName, IEnumerable<string> context)
        => _megaTextureNotFound.Create(verifier, $"Cannot find CommandBar MegaTexture '{baseName}.tga'.", $"{baseName}.tga", context);
}
