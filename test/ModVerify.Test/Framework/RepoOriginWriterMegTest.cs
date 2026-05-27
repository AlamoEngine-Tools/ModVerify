using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Files.MEG.Services;
using Xunit;

namespace ModVerify.Test.Framework;

public class RepoOriginWriterMegTest : ModVerifyTestBase
{
    [Fact]
    public void WriteMeg_ForwardSlashEntryName_NormalizedToUppercaseBackslash()
    {
        var megService = ServiceProvider.GetRequiredService<IMegFileService>();
        var fs = ServiceProvider.GetRequiredService<IFileSystem>();

        using var repo = CreateBuilder()
            .WithGame(g => g.WriteMeg(
                "Data/test.meg",
                meg => meg.Add("scripts/init.lua", "print('hello')")))
            .Build();

        var megPath = fs.Path.Combine(repo.GameLocations.GamePath, "Data", "test.meg");
        var loaded = megService.Load(megPath);

        var entry = Assert.Single(loaded.Archive);
        Assert.Equal(@"SCRIPTS\INIT.LUA", entry.Path);
    }
}
