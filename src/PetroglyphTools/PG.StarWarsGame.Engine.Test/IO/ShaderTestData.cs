using System.Collections.Generic;

namespace PG.StarWarsGame.Engine.Test.IO;

/// <summary>
/// Shared theory data for shader lookup tests.
/// The engine strips the input's extension (anything after the last '.' in the filename portion)
/// before probing, so a shader request named "MyShader.X" should resolve the same way regardless
/// of <c>X</c>. The engine dimension comes from the concrete test class.
/// </summary>
internal static class ShaderTestData
{
    /// <summary>All shader-name forms that should be treated as equivalent (strip to "MyShader").</summary>
    public static readonly string[] EquivalentShaderNames =
    [
        "MyShader",
        "MyShader.fx",
        "MyShader.fxo",
        "MyShader.fxh",
        "MyShader.bogus",
    ];

    public static IEnumerable<object[]> Inputs()
    {
        foreach (var input in EquivalentShaderNames)
            yield return [input];
    }
}
