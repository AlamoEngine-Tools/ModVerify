using System;
using System.Reflection;
using PG.StarWarsGame.Engine.Testing;

namespace ModVerify.Test.Framework;

/// <summary>Provides the minimal FoC skeleton as an extension on <see cref="VirtualGameRepoBuilder"/>.</summary>
public static class MinimalFoc
{
    private static readonly Assembly Asm = typeof(MinimalFoc).Assembly;

    /// <summary>Scaffolds the committed minimal FoC skeleton onto the builder's game origin.</summary>
    /// <remarks>
    /// Empty MEG archives are produced at scaffold time using the production MEG writer; the resulting bytes are written through the builder.
    /// Hand-authoring binary MEG files in <c>Fixtures/</c> would require committing platform-specific bytes.
    /// </remarks>
    /// <param name="builder">The builder to populate.</param>
    public static VirtualGameRepoBuilder WithMinimalFoc(this VirtualGameRepoBuilder builder)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        return builder.WithGame(g =>
        {
            g.WriteEmptyMeg("Data/Patch.meg");
            g.WriteEmptyMeg("Data/Patch2.meg");
            g.WriteEmptyMeg("Data/64Patch.meg");
            g.WriteEmptyMeg("Data/Audio/SFX/SFX2D_NON_LOCALIZED.MEG");
            g.WriteEmptyMeg("Data/Audio/SFX/SFX3D_NON_LOCALIZED.MEG");

            g.WriteEmbeddedTree("MinimalFoc", Asm);
        });
    }
}
