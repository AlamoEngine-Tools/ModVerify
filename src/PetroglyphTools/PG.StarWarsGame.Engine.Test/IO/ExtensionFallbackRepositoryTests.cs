using Xunit;

namespace PG.StarWarsGame.Engine.Test.IO;

public abstract class ExtensionFallbackRepositoryTests : EngineRepositoryTestBase
{
    /// <summary>The extension a request falls back to when its own extension is not present.</summary>
    protected abstract string FallbackExtension { get; }

    /// <summary>A supported extension that resolves by its own name but is never a fallback target.</summary>
    protected abstract string SecondaryExtension { get; }

    [Fact]
    public void FileExists_EachSupportedExtension_ResolvesByItsOwnName()
    {
        var select = RepositoryFixture.SelectRepository;
        using var repo = CreateBuilder()
            .ConfigureGame(g =>
            {
                g.Write(AssetPath(FallbackExtension), "fallback");
                g.Write(AssetPath(SecondaryExtension), "secondary");
            })
            .Build();
        var repoUnderTest = select(CreateRepository(repo));

        Assert.True(repoUnderTest.FileExists(AssetPath(FallbackExtension)));
        Assert.Equal("fallback", ReadAll(repoUnderTest.OpenFile(AssetPath(FallbackExtension))));
        Assert.True(repoUnderTest.FileExists(AssetPath(SecondaryExtension)));
        Assert.Equal("secondary", ReadAll(repoUnderTest.OpenFile(AssetPath(SecondaryExtension))));
    }

    [Fact]
    public void FileExists_UnsupportedExtension_FallsBackToFallbackExtension()
    {
        var select = RepositoryFixture.SelectRepository;
        using var repo = CreateBuilder()
            .ConfigureGame(g => g.Write(AssetPath(FallbackExtension), "fallback"))
            .Build();
        var repoUnderTest = select(CreateRepository(repo));

        Assert.True(repoUnderTest.FileExists(AssetPath(".unsupported")));
    }

    [Fact]
    public void FileExists_UnsupportedExtension_FallsBackToFallbackExtensionInMeg()
    {
        var select = RepositoryFixture.SelectRepository;
        using var repo = CreateBuilder()
            .ConfigureGame(g => g.RegisterAndWriteMeg("Data/Assets.meg",
                meg => meg.Add(AssetPath(FallbackExtension), "fallback")))
            .Build();
        var repoUnderTest = select(CreateRepository(repo));

        Assert.True(repoUnderTest.FileExists(AssetPath(".unsupported")));
    }

    [Fact]
    public void FileExists_UnsupportedExtension_DoesNotResolveSecondaryFile()
    {
        var select = RepositoryFixture.SelectRepository;
        using var repo = CreateBuilder()
            .ConfigureGame(g => g.Write(AssetPath(SecondaryExtension), "secondary"))
            .Build();
        var repoUnderTest = select(CreateRepository(repo));

        Assert.False(repoUnderTest.FileExists(AssetPath(".unsupported")));
    }

    [Fact]
    public void FileExists_ExtensionlessRequest_FallsBackToFallbackExtension()
    {
        var select = RepositoryFixture.SelectRepository;
        using var repo = CreateBuilder()
            .ConfigureGame(g => g.Write(AssetPath(FallbackExtension), "fallback"))
            .Build();
        var repoUnderTest = select(CreateRepository(repo));

        Assert.True(repoUnderTest.FileExists(AssetPath(string.Empty)));
    }

    [Fact]
    public void FileExists_FileNameWithoutDirectory_FallsBackWhenSupported()
    {
        // Resolving a bare name with a non-fallback extension needs both the implicit directory and the
        // extension fallback, so it succeeds only where the repository has an implicit directory.
        var select = RepositoryFixture.SelectRepository;
        using var repo = CreateBuilder()
            .ConfigureGame(g => g.Write(AssetPath(FallbackExtension), "fallback"))
            .Build();
        var repoUnderTest = select(CreateRepository(repo));

        var bareName = FileSystem.Path.GetFileName(AssetPath(SecondaryExtension));
        Assert.Equal(ResolvesFileNameWithoutDirectory, repoUnderTest.FileExists(bareName));
    }

    [Fact]
    public void Priority_ExactExtensionBeatsFallbackExtension_SameOrigin()
    {
        var select = RepositoryFixture.SelectRepository;
        using var repo = CreateBuilder()
            .ConfigureGame(g =>
            {
                g.Write(AssetPath(SecondaryExtension), "secondary");
                g.Write(AssetPath(FallbackExtension), "fallback");
            })
            .Build();
        var repoUnderTest = select(CreateRepository(repo));

        Assert.Equal("secondary", ReadAll(repoUnderTest.OpenFile(AssetPath(SecondaryExtension))));
    }

    [Fact]
    public void Priority_ExactExtensionInFallbackOrigin_BeatsFallbackExtensionInMod()
    {
        // The exact-extension pass walks the whole chain before the fallback-extension pass, so an exact hit
        // in the fallback origin outranks a fallback-extension hit in a mod — extension dominates chain position.
        var select = RepositoryFixture.SelectRepository;
        using var repo = CreateBuilder()
            .WithMod("Mod", m => m.Write(AssetPath(FallbackExtension), "mod-fallback"))
            .WithFallbackGame(f => f.Write(AssetPath(SecondaryExtension), "fb-secondary"))
            .Build();
        var repoUnderTest = select(CreateRepository(repo));

        Assert.Equal("fb-secondary", ReadAll(repoUnderTest.OpenFile(AssetPath(SecondaryExtension))));
    }

    [Fact]
    public void Priority_FallbackExtension_RespectsChainOrder()
    {
        // With no exact match anywhere, the fallback-extension pass still honors the chain: a mod's copy wins.
        var select = RepositoryFixture.SelectRepository;
        using var repo = CreateBuilder()
            .WithMod("Mod", m => m.Write(AssetPath(FallbackExtension), "mod-fallback"))
            .ConfigureGame(g => g.Write(AssetPath(FallbackExtension), "game-fallback"))
            .Build();
        var repoUnderTest = select(CreateRepository(repo));

        Assert.Equal("mod-fallback", ReadAll(repoUnderTest.OpenFile(AssetPath(".unsupported"))));
    }

    private string AssetPath(string extension)
    {
        var resolvable = RepositoryFixture.ResolvablePath;
        return resolvable.Substring(0, resolvable.LastIndexOf('.')) + extension;
    }
}
