namespace AET.ModVerify.Reporting;

public enum VerificationSeverity
{
    /// <summary>
    /// Indicates that a finding is most likely not affecting the game.
    /// </summary>
    Information = 0,
    /// <summary>
    /// Indicates that a finding might cause undefined behavior or unpredictable results are to be expected.
    /// </summary>
    Warning,
    /// <summary>
    /// Indicates that a finding will most likely not function as expected in the game.
    /// </summary>
    Error,
    /// <summary>
    /// Indicates that a finding will most likely cause a CTD of the game.
    /// </summary>
    Critical
}