using System.Collections.Generic;

namespace PG.StarWarsGame.Engine.Test;

/// <summary>Shared theory data for repository lookup tests.</summary>
internal static class RepositoryTestData
{
    /// <summary>
    /// All shader-name forms that resolve identically: the effects repository strips the input's extension
    /// before probing, so "MyShader.X" resolves the same regardless of <c>X</c>.
    /// </summary>
    public static readonly string[] EquivalentShaderNames =
    [
        "MyShader",
        "MyShader.fx",
        "MyShader.fxo",
        "MyShader.fxh",
        "MyShader.bogus",
    ];

    public static IEnumerable<object[]> ShaderInputs()
    {
        foreach (var input in EquivalentShaderNames)
            yield return [input];
    }
}
