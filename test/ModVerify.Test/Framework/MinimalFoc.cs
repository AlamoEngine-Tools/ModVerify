using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.Testing;
using PG.StarWarsGame.Files.MEG.Services;

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
    /// <param name="services">A service provider supplying file-format helpers (MEG creation).</param>
    public static VirtualGameRepoBuilder WithMinimalFoc(
        this VirtualGameRepoBuilder builder, IServiceProvider services)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        var megService = services.GetRequiredService<IMegFileService>();

        return builder.WithGame(g =>
        {
            g.WriteEmptyMeg("Data/Patch.meg", megService);
            g.WriteEmptyMeg("Data/Patch2.meg", megService);
            g.WriteEmptyMeg("Data/64Patch.meg", megService);
            g.WriteEmptyMeg("Data/Audio/SFX/SFX2D_NON_LOCALIZED.MEG", megService);
            g.WriteEmptyMeg("Data/Audio/SFX/SFX3D_NON_LOCALIZED.MEG", megService);

            g.WriteEmbeddedTree("MinimalFoc", Asm);
        });
    }
}
