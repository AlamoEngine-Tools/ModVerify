using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using AnakinRaW.CommonUtilities.Testing.Extensions;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Engine.IO.Repositories;
using PG.StarWarsGame.Engine.Testing;
using Testably.Abstractions;
using Xunit;

namespace PG.StarWarsGame.Engine.Test;

/// <summary>
/// Base class for engine-bound repository tests.
/// </summary>
public abstract class EngineRepositoryTestBase : EngineTestBase
{
    /// <summary>Gets the engine targeted by this test class.</summary>
    protected abstract GameEngineType Engine { get; }

    /// <summary>Whether the repository resolves a file requested by name only, without its directory.</summary>
    protected abstract bool ResolvesFileNameWithoutDirectory { get; }

    /// <summary>Whether the repository surfaces the path-too-long condition for an overlong request.</summary>
    protected abstract bool SurfacesPathTooLong { get; }

    /// <summary>
    /// The repository origins in descending lookup priority for this test class's <see cref="Engine"/>.
    /// </summary>
    private IReadOnlyList<RepositoryLayer> ExpectedLoadOrder => Engine switch
    {
        GameEngineType.Foc =>
        [
            RepositoryLayer.Mod,
            RepositoryLayer.Game,
            RepositoryLayer.MasterMeg,
            RepositoryLayer.Fallback,
        ],
        GameEngineType.Eaw =>
        [
            RepositoryLayer.Mod,
            RepositoryLayer.Game,
            RepositoryLayer.Fallback,
            RepositoryLayer.MasterMeg,
        ],
        _ => throw new ArgumentOutOfRangeException()
    };

    /// <summary>
    /// Builds a <see cref="CaseInsensitivityFixture"/> instance that provides test data and logic 
    /// for verifying case and separator insensitivity in repository lookups.
    /// </summary>
    /// <remarks>
    /// Path casing and separators automatically randomized by the underlying test logic.
    /// </remarks>
    /// <returns>
    /// A <see cref="CaseInsensitivityFixture"/> containing the configuration and data 
    /// necessary for case insensitivity tests.
    /// </returns>
    protected abstract CaseInsensitivityFixture BuildCaseInsensitivityFixture();

    /// <summary>A simple fixture describing basic file existence tests for this test class's repository.</summary>
    protected abstract RepositoryFixture RepositoryFixture { get; }

    protected sealed override IFileSystem CreateFileSystem()
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
    {
        return CreateRepository(Engine, repo, errorReporter: null);
    }

    /// <summary>Constructs an <see cref="IGameRepository"/> for this test class's <see cref="Engine"/> with a custom error reporter
    /// to observe init-time assertions (e.g. <see cref="EngineAssertKind.FileNotFound"/> for missing patches).</summary>
    protected IGameRepository CreateRepository(VirtualGameRepo repo, IGameEngineErrorReporter? errorReporter)
    {
        return CreateRepository(Engine, repo, errorReporter);
    }

    private GameRepository CreateRepository(GameEngineType engine, VirtualGameRepo repo, IGameEngineErrorReporter? errorReporter)
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
    
    public static TheoryData<PetroglyphFileSystemStrategy> SupportedFileSystemStrategies()
    {
        var data = new TheoryData<PetroglyphFileSystemStrategy>();
        foreach (var strategy in PetroglyphFileSystemTestHelpers.SupportedForCurrentOS())
            data.Add(strategy);
        return data;
    }

    [Theory]
    [MemberData(nameof(SupportedFileSystemStrategies))]
    public void Lookup_IsCaseAndSeparatorInsensitive_AcrossFilesystemAndMeg(PetroglyphFileSystemStrategy strategy)
    {
        var fixture = BuildCaseInsensitivityFixture();

        using var virt = CreateBuilder()
            .ConfigureGame(fixture.PopulateGame)
            .Build();
        var gameRepo = CreateRepository(virt);
        gameRepo.PGFileSystem.ApplyStrategy(strategy);
        var repoUnderTest = fixture.SelectRepository(gameRepo);

        var separatorRandom = new Random(42);
        for (var i = 0; i < 32; i++)
        {
            var fsVariant = JitterSeparators(string.ShuffleCasing(fixture.FilesystemLookup), separatorRandom);
            var megVariant = JitterSeparators(string.ShuffleCasing(fixture.MegLookup), separatorRandom);

            Assert.True(repoUnderTest.FileExists(fsVariant), $"Filesystem variant '{fsVariant}' should resolve.");
            Assert.True(repoUnderTest.FileExists(megVariant), $"MEG variant '{megVariant}' should resolve.");
            Assert.Equal(fixture.FilesystemContent, ReadAll(repoUnderTest.OpenFile(fsVariant)));
            Assert.Equal(fixture.MegContent, ReadAll(repoUnderTest.OpenFile(megVariant)));
        }
    }

    #region Asset existence

    public static TheoryData<RepositoryLayer> AllOrigins()
    {
        return [RepositoryLayer.Mod, RepositoryLayer.Game, RepositoryLayer.MasterMeg, RepositoryLayer.Fallback];
    }

    [Theory]
    [MemberData(nameof(AllOrigins))]
    public void FileExists_ResolvesFromOrigin(RepositoryLayer origin)
    {
        var fixture = RepositoryFixture;

        var builder = CreateBuilder();
        WriteLayer(builder, origin, fixture.ResolvablePath, "content");
        using var repo = builder.Build();
        var repoUnderTest = fixture.SelectRepository(CreateRepository(repo));

        Assert.True(repoUnderTest.FileExists(fixture.ResolvablePath));
    }

    [Theory]
    [MemberData(nameof(AllOrigins))]
    public void FileExists_EmptyPath_ReturnsFalse(RepositoryLayer origin)
    {
        var fixture = RepositoryFixture;

        var builder = CreateBuilder();
        WriteLayer(builder, origin, fixture.ResolvablePath, "content");
        using var repo = builder.Build();
        var repoUnderTest = fixture.SelectRepository(CreateRepository(repo));

        Assert.False(repoUnderTest.FileExists(""));
    }

    [Theory]
    [MemberData(nameof(AllOrigins))]
    public void FileExists_NullPath_ReturnsFalse(RepositoryLayer origin)
    {
        var fixture = RepositoryFixture;

        var builder = CreateBuilder();
        WriteLayer(builder, origin, fixture.ResolvablePath, "content");
        using var repo = builder.Build();
        var repoUnderTest = fixture.SelectRepository(CreateRepository(repo));

        Assert.False(repoUnderTest.FileExists(null!));
    }

    [Fact]
    public void FileExists_MissingAsset_ReturnsFalse()
    {
        var fixture = RepositoryFixture;

        using var repo = CreateBuilder().Build();
        var repoUnderTest = fixture.SelectRepository(CreateRepository(repo));

        Assert.False(repoUnderTest.FileExists(fixture.ResolvablePath));
    }

    [Fact]
    public void FileExists_FileNameWithoutDirectory_ResolvesWhenSupported()
    {
        // The file is written at its full path; whether its name alone resolves depends on the repository
        // prepending a built-in directory (effects, textures) or not (models, base lookup).
        var fixture = RepositoryFixture;
        var fileName = FileSystem.Path.GetFileName(fixture.ResolvablePath);

        using var repo = CreateBuilder()
            .ConfigureGame(g => g.Write(fixture.ResolvablePath, "content"))
            .Build();
        var repoUnderTest = fixture.SelectRepository(CreateRepository(repo));

        Assert.Equal(ResolvesFileNameWithoutDirectory, repoUnderTest.FileExists(fileName));
    }

    [Fact]
    public void FileExists_OverlongPath_IsMissingAndFlagsPathTooLongWhenSupported()
    {
        var fixture = RepositoryFixture;

        using var repo = CreateBuilder().Build();
        var repoUnderTest = fixture.SelectRepository(CreateRepository(repo));

        var overlong = new string('a', 300);
        var found = repoUnderTest.FileExists(overlong.AsSpan(), megFileOnly: false, out var pathTooLong);

        Assert.False(found);
        Assert.Equal(SurfacesPathTooLong, pathTooLong);
    }

    #endregion

    #region Default loading chain priority

    [Theory]
    [MemberData(nameof(SupportedFileSystemStrategies))]
    public void Priority_ResolvesAccordingToEngineLoadOrder(PetroglyphFileSystemStrategy strategy)
    {
        var fixture = RepositoryFixture;
        var order = ExpectedLoadOrder;

        // Sliding 'top' down the list makes each origin,
        // in turn, the highest-priority one holding the file (Example for FOC):
        //   top = mod      -> all four origins hold it      -> mod must win
        //   top = game     -> game, MEG, fallback hold it   -> game must win  (mod is empty)
        //   top = MEG      -> MEG, fallback hold it          -> MEG must win   (mod, game empty)
        //   top = fallback -> only fallback holds it         -> fallback wins
        for (var top = 0; top < order.Count; top++)
        {
            var builder = CreateBuilder();
            for (var i = top; i < order.Count; i++)
                WriteLayer(builder, order[i], fixture.ResolvablePath, order[i].ToString());

            using var repo = builder.Build();
            var gameRepo = CreateRepository(repo);
            gameRepo.PGFileSystem.ApplyStrategy(strategy);
            var repoUnderTest = fixture.SelectRepository(gameRepo);

            var winner = order[top];
            Assert.Equal(winner.ToString(), ReadAll(repoUnderTest.OpenFile(fixture.ResolvablePath)));
        }
    }

    private static void WriteLayer(VirtualGameRepoBuilder builder, RepositoryLayer layer, string path, string content)
    {
        switch (layer)
        {
            case RepositoryLayer.Mod:
                builder.WithMod("Mod", w => w.Write(path, content));
                break;
            case RepositoryLayer.Game:
                builder.ConfigureGame(g => g.Write(path, content));
                break;
            case RepositoryLayer.MasterMeg:
                builder.ConfigureGame(g => g.WriteMeg("Data/Patch.meg", meg => meg.Add(path, content)));
                break;
            case RepositoryLayer.Fallback:
                builder.WithFallbackGame(f => f.Write(path, content));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(layer), layer, null);
        }
    }

    [Fact]
    public void Priority_ModDeclarationOrderIsRespected()
    {
        var fixture = RepositoryFixture;

        using var repo = CreateBuilder()
            .WithMod("ModA", m => m.Write(fixture.ResolvablePath, "A"))
            .WithMod("ModB", m => m.Write(fixture.ResolvablePath, "B"))
            .Build();
        var repoUnderTest = fixture.SelectRepository(CreateRepository(repo));

        Assert.Equal("A", ReadAll(repoUnderTest.OpenFile(fixture.ResolvablePath)));
    }

    [Fact]
    public void Priority_FallbackDeclarationOrderIsRespected()
    {
        var fixture = RepositoryFixture;

        using var repo = CreateBuilder()
            .WithFallback("FallbackA", w => w.Write(fixture.ResolvablePath, "A"))
            .WithFallback("FallbackB", w => w.Write(fixture.ResolvablePath, "B"))
            .Build();
        var repoUnderTest = fixture.SelectRepository(CreateRepository(repo));

        Assert.Equal("A", ReadAll(repoUnderTest.OpenFile(fixture.ResolvablePath)));
    }

    #endregion

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

    /// <summary>
    /// Returns the last segment of an engine path, treating both '/' and '\' as separators.
    /// </summary>
    // Engine paths always use '\'; System.IO.Path.GetFileName only splits on '\' on Windows.
    protected static string EngineFileName(string path)
    {
        return path.Substring(path.LastIndexOfAny(['/', '\\']) + 1);
    }
}
