namespace PG.StarWarsGame.Engine.Test;

/// <summary>
/// Specifies an origin layer of the repository loading chain.
/// </summary>
/// <remarks>
/// The relative priority of these origins is engine-specific.
/// </remarks>
public enum RepositoryLayer
{
    /// <summary>A mod path.</summary>
    Mod,

    /// <summary>The base game directory.</summary>
    Game,

    /// <summary>The master MEG archive.</summary>
    MasterMeg,

    /// <summary>A fallback game directory.</summary>
    Fallback,
}