using AET.ModVerify.App.Settings.CommandLine;
using AnakinRaW.ApplicationBase.Environment;
using System;
using System.IO.Abstractions;
using ModVerify.CliApp.Test.TestData;
using Testably.Abstractions;
#if NETFRAMEWORK
using ModVerify.CliApp.Test.Utilities;
#endif


namespace ModVerify.CliApp.Test;

public class ModVerifyOptionsParserTest_Updateable : ModVerifyOptionsParserTestBase
{
    protected override bool IsUpdatable => true;

    protected override ApplicationEnvironment CreateEnvironment()
    {
        return new UpdatableEnv(GetType().Assembly, FileSystem);
    }

    [Fact]
    public void Parse_UpdateAppArg()
    {
        const string argString = "updateApplication --updateBranch test --updateManifestUrl https://examlple.com";

        var settings = Parser.Parse(argString.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        Assert.True(settings.HasOptions);
        Assert.Null(settings.ModVerifyOptions);
        Assert.NotNull(settings.UpdateOptions);
        Assert.Equal("test", settings.UpdateOptions.BranchName);
        Assert.Equal("https://examlple.com", settings.UpdateOptions.ManifestUrl);
    }

    [Fact]
    public void Parse_CombinedIsNotAllowed()
    {
        const string argString = "verify --updateBranch test --updateManifestUrl https://examlple.com";

        var settings = Parser.Parse(argString.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        Assert.False(settings.HasOptions);
        Assert.Null(settings.ModVerifyOptions);
        Assert.Null(settings.UpdateOptions);
    }
}

public class ModVerifyOptionsParserTest_NotUpdateable : ModVerifyOptionsParserTestBase
{
    protected override bool IsUpdatable => false;

    protected override ApplicationEnvironment CreateEnvironment()
    {
        return new TestEnv(GetType().Assembly, FileSystem);
    }

    [Theory]
    [InlineData("verify --externalUpdaterResult UpdateSuccess")]
    [InlineData("createBaseline --externalUpdaterResult UpdateSuccess")]
    [InlineData("verify --junkOption")]
    [InlineData("createBaseline --junkOption")]
    [InlineData("updateApplication")]
    [InlineData("updateApplication --updateBranch test --updateManifestUrl https://examlple.com")]
    public void Parse_InvalidArgs_NotUpdateable(string argString)
    {
        var settings = Parser.Parse(argString.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        Assert.False(settings.HasOptions);
        Assert.Null(settings.ModVerifyOptions);
        Assert.Null(settings.UpdateOptions);
    }
}


public abstract class ModVerifyOptionsParserTestBase
{
    private protected readonly ModVerifyOptionsParser Parser;
    protected readonly IFileSystem FileSystem = new RealFileSystem();

    protected abstract ApplicationEnvironment CreateEnvironment();

    protected abstract bool IsUpdatable { get; }

    protected ModVerifyOptionsParserTestBase()
    {
        Parser = new ModVerifyOptionsParser(CreateEnvironment(), null);
    }

    [Fact]
    public void Parse_NoArgs_IsVerify_IsInteractive()
    {
        var settings = Parser.Parse([]);

        Assert.True(settings.HasOptions);
        var verify = Assert.IsType<VerifyVerbOption>(settings.ModVerifyOptions);
        Assert.True(verify.IsRunningWithoutArguments);
        Assert.Null(settings.UpdateOptions);
    }

    [Theory]
    [InlineData("verify", false)]
    [InlineData("verify -v", false)]
    [InlineData("createBaseline -o out.json", true)]
    [InlineData("createBaseline -v -o out.json", true)]
    public void Parse_Interactive(string argString, bool createBaseLine)
    {
        var settings = Parser.Parse(argString.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        Assert.True(settings.HasOptions);
        if (createBaseLine)
        {
            Assert.IsType<CreateBaselineVerbOption>(settings.ModVerifyOptions);
        }
        else
        {
            var verify = Assert.IsType<VerifyVerbOption>(settings.ModVerifyOptions);
            Assert.False(verify.IsRunningWithoutArguments);
        }
        Assert.Null(settings.UpdateOptions);
    }

    [Theory]
    [InlineData("verify --path myMod", false)]
    [InlineData("verify -v --game myGame", false)]
    [InlineData("createBaseline -o out.json --path myMod", true)]
    [InlineData("createBaseline -v -o out.json --game myGame", true)]
    public void Parse_NotInteractive(string argString, bool createBaseLine)
    {
        var settings = Parser.Parse(argString.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        Assert.True(settings.HasOptions);
        Assert.NotNull(settings.ModVerifyOptions);

        if (createBaseLine)
            Assert.IsType<CreateBaselineVerbOption>(settings.ModVerifyOptions);
        else
        {
            var verify = Assert.IsType<VerifyVerbOption>(settings.ModVerifyOptions);
            Assert.False(verify.IsRunningWithoutArguments);
        }

        Assert.Null(settings.UpdateOptions);
    }

    [Theory]
    [InlineData("verify --path myMod --game myGame")]
    [InlineData("verify --game myMod --path myMod")]
    [InlineData("verify --mod myMod --path myMod")]
    [InlineData("verify --fallbackGame myGame --path myMod")]
    public void Parse_InvalidPathConfig(string argString)
    {
        var settings = Parser.Parse(argString.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        Assert.False(settings.HasOptions);
        Assert.Null(settings.ModVerifyOptions);
        Assert.Null(settings.UpdateOptions);
    }

    [Theory]
    [InlineData("")]
    [InlineData("junkVerb")]
    [InlineData("junkVerb verify")]
    [InlineData("junkVerb verify --v")]
    [InlineData("junkVerb --v")]
    [InlineData("verify --junkOption")]
    [InlineData("verify -v --junkOption")]
    [InlineData("updateApplication --junkOption")]
    [InlineData("--junkOption")]
    [InlineData("junkVerb --junkOption")]
    [InlineData("junkVerb --externalUpdaterResult UpdateSuccess")]
    [InlineData("-v")] 
    public void Parse_InvalidArgs(string argString)
    {
        var settings = Parser.Parse(argString.Split(' '));

        Assert.False(settings.HasOptions);
        Assert.Null(settings.ModVerifyOptions);
        Assert.Null(settings.UpdateOptions);
    }

    [Fact]
    public void Parse_UpdatePerformed_RestartedFromNoArgs()
    {
        // This only happens when we run without args, performed an auto-update and restarted the application automatically.
        const string argString = "--externalUpdaterResult UpdateSuccess";

        var settings = Parser.Parse(argString.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        if (!IsUpdatable)
            Assert.False(settings.HasOptions);
        else
        {
            Assert.True(settings.HasOptions);
            var verify = Assert.IsType<VerifyVerbOption>(settings.ModVerifyOptions);
            Assert.True(verify.IsRunningWithoutArguments);
            Assert.Null(settings.UpdateOptions);
        }
    }

    [Theory]
    [InlineData("createBaseline")]
    [InlineData("createBaseline -v")]
    public void Parse_CreateBaseline_MissingRequired_Fails(string argString)
    {
        var settings = Parser.Parse(argString.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        Assert.False(settings.HasOptions);
        Assert.Null(settings.ModVerifyOptions);
        Assert.Null(settings.UpdateOptions);
    }
}