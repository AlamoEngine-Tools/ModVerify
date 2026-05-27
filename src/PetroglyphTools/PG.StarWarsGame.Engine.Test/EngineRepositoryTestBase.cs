using System;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using AnakinRaW.CommonUtilities.Hashing;
using AnakinRaW.CommonUtilities.Testing;
using AnakinRaW.CommonUtilities.Testing.Extensions;
using Microsoft.Extensions.DependencyInjection;
using PG.Commons;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Engine.Testing;
using PG.StarWarsGame.Files.ALO;
using PG.StarWarsGame.Files.MEG;
using PG.StarWarsGame.Files.MTD;
using PG.StarWarsGame.Files.XML;
using Testably.Abstractions;
using Xunit;

namespace PG.StarWarsGame.Engine.Test;

/// <summary>
/// Base class for engine-bound repository tests.
/// One concrete subclass per <see cref="GameEngineType"/> declares the engine via <see cref="Engine"/>;
/// tests defined on the abstract per-category base classes (e.g. <c>GameRepositoryFileLookupTests</c>) are
/// discovered through inheritance and run once per engine-specific subclass.
/// </summary>
public abstract class EngineRepositoryTestBase : TestBaseWithFileSystem
{
    /// <summary>The engine targeted by this test class. Each concrete leaf class declares its engine here.</summary>
    protected abstract GameEngineType Engine { get; }

    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);

        serviceCollection.AddSingleton<IHashingService>(sp => new HashingService(sp));

        serviceCollection.SupportMTD();
        serviceCollection.SupportMEG();
        serviceCollection.SupportALO();
        serviceCollection.SupportXML();
        PetroglyphCommons.ContributeServices(serviceCollection);
        PetroglyphEngineServiceContribution.ContributeServices(serviceCollection);
    }

    protected override IFileSystem CreateFileSystem()
    {
        // Real file system is required to test integration
        // with PG.StarWarsGame.Engine.FileSystem
        return new RealFileSystem();
    }

    /// <summary>Creates a builder bound to the test base's service provider.</summary>
    protected VirtualGameRepoBuilder CreateBuilder()
    {
        return new VirtualGameRepoBuilder(ServiceProvider);
    }

    /// <summary>Constructs an <see cref="IGameRepository"/> for this test class's <see cref="Engine"/>.</summary>
    /// <remarks>The returned repository is sealed against further MEG modifications, matching the engine-init lifecycle.</remarks>
    protected IGameRepository CreateRepository(VirtualGameRepo repo)
        => CreateRepository(Engine, repo, errorReporter: null);

    /// <summary>Constructs an <see cref="IGameRepository"/> for this test class's <see cref="Engine"/> with a custom error reporter
    /// to observe init-time assertions (e.g. <see cref="EngineAssertKind.FileNotFound"/> for missing patches).</summary>
    protected IGameRepository CreateRepository(VirtualGameRepo repo, IGameEngineErrorReporter? errorReporter)
        => CreateRepository(Engine, repo, errorReporter);

    /// <summary>Constructs an <see cref="IGameRepository"/> for an explicit engine. Use only when a test must
    /// exercise a non-current engine (e.g. asserting factory dispatch for another engine).</summary>
    protected IGameRepository CreateRepository(GameEngineType engine, VirtualGameRepo repo)
        => CreateRepository(engine, repo, errorReporter: null);

    private IGameRepository CreateRepository(GameEngineType engine, VirtualGameRepo repo, IGameEngineErrorReporter? errorReporter)
    {
        if (repo == null)
            throw new ArgumentNullException(nameof(repo));

        var factory = new GameRepositoryFactory(ServiceProvider);
        var wrapper = new GameEngineErrorReporterWrapper(errorReporter);
        var gameRepo = factory.Create(engine, repo.GameLocations, wrapper);
        gameRepo.Seal();
        return gameRepo;
    }

    /// <summary>Reads the stream to end as UTF-8 text and disposes it. Convenient for inspecting <c>OpenFile</c> results.</summary>
    protected static string ReadAll(Stream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        using (stream)
        using (var reader = new StreamReader(stream, Encoding.UTF8))
            return reader.ReadToEnd();
    }

    /// <summary>
    /// Describes the repository facet under test and the fixtures used by the inherited
    /// <see cref="Lookup_IsCaseAndSeparatorInsensitive_AcrossFilesystemAndMeg"/> test.
    /// The default targets the base <see cref="IGameRepository"/>; derived classes override
    /// to target their specialized facet (Effects, Texture, Model).
    /// </summary>
    protected virtual RepositoryLookupSetup GetLookupSetup() => new(
        PopulateGame: g =>
        {
            g.Write("Data/XML/Foo.xml", "fs-content");
            g.WriteMeg("Data/Patch.meg", meg => meg.Add("Data/Audio/Bar.wav", "meg-content"));
        },
        SelectRepository: gameRepo => gameRepo,
        FilesystemLookup: "Data/XML/Foo.xml",
        FilesystemContent: "fs-content",
        MegLookup: "Data/Audio/Bar.wav",
        MegContent: "meg-content");

    /// <summary>
    /// Sanity check that lookups through the engine layer remain case- and separator-insensitive for both
    /// filesystem-backed and MEG-backed files, against whichever <see cref="IRepository"/> facet the
    /// derived class exercises.
    /// </summary>
    /// <remarks>
    /// Defined on the base class so xUnit discovers it in every concrete repository-test class. Case shuffling
    /// uses <see cref="StringExtensions.ShuffleCasing(string)"/>; separator shuffling uses a seeded local
    /// <see cref="Random"/> so a separator-related failure stays reproducible.
    /// </remarks>
    [Fact]
    public void Lookup_IsCaseAndSeparatorInsensitive_AcrossFilesystemAndMeg()
    {
        var setup = GetLookupSetup();

        using var virt = CreateBuilder()
            .WithGame(setup.PopulateGame)
            .Build();
        var gameRepo = CreateRepository(virt);
        var repoUnderTest = setup.SelectRepository(gameRepo);

        var separatorRandom = new Random(42);
        for (var i = 0; i < 32; i++)
        {
            var fsVariant = JitterSeparators(string.ShuffleCasing(setup.FilesystemLookup), separatorRandom);
            var megVariant = JitterSeparators(string.ShuffleCasing(setup.MegLookup), separatorRandom);

            Assert.True(repoUnderTest.FileExists(fsVariant), $"Filesystem variant '{fsVariant}' should resolve.");
            Assert.True(repoUnderTest.FileExists(megVariant), $"MEG variant '{megVariant}' should resolve.");
            Assert.Equal(setup.FilesystemContent, ReadAll(repoUnderTest.OpenFile(fsVariant)));
            Assert.Equal(setup.MegContent, ReadAll(repoUnderTest.OpenFile(megVariant)));
        }
    }

    private static string JitterSeparators(string path, Random random)
    {
        var chars = path.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (chars[i] == '/' || chars[i] == '\\')
                chars[i] = random.Next(2) == 0 ? '/' : '\\';
        }
        return new string(chars);
    }
}
