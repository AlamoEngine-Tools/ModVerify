namespace AET.ModVerify.Settings;

/// <summary>
/// Represents the settings for an <see cref="IGameVerifierService"/>,
/// which controls the execution of game verifiers and their interactions with the game engine and reporting mechanisms.
/// </summary>
public sealed class VerifierServiceSettings
{
    /// <summary>
    /// Gets or sets the <see cref="GameVerifySettings"/> that specify the settings for game verification.
    /// </summary>
    public required GameVerifySettings GameVerifySettings { get; init; }
    
    /// <summary>
    /// Gets or sets the provider that supplies the game verifiers.
    /// </summary>
    public required IGameVerifiersProvider VerifiersProvider { get; init; }

    /// <summary>
    /// Gets or sets the <see cref="FailFastSetting"/> that specify the fail-fast behavior of the verification process.
    /// </summary>
    public FailFastSetting FailFastSettings { get; init; } = FailFastSetting.NoFailFast;

    /// <summary>
    /// Gets or sets the number of parallel verifiers to run during the verification process.
    /// </summary>
    public int ParallelVerifiers { get; init; } = 4;

    /// <summary>
    /// Gets or sets a value indicating whether the game engine should be configured
    /// to use a live virtual file system strategy during verification.
    /// </summary>
    /// <remarks>
    /// A live virtual file system strategy is able to detect changes to the file system in real time.
    /// </remarks>
    public bool UseLiveVirtualFileSystem { get; init; } = false;
}