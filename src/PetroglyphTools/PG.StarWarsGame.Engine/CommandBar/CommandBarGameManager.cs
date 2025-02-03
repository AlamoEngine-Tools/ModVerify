﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.Commons.Collections;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.CommandBar.Xml;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Engine.Database.ErrorReporting;
using PG.StarWarsGame.Engine.IO.Repositories;
using PG.StarWarsGame.Engine.Xml.Parsers;

namespace PG.StarWarsGame.Engine.CommandBar;

public interface ICommandBarGameManager : IGameManager<CommandBarComponentData>
{
}

internal class CommandBarGameManager(
    GameRepository repository,
    DatabaseErrorListenerWrapper errorListener,
    IServiceProvider serviceProvider)
    : GameManagerBase<CommandBarComponentData>(repository, errorListener, serviceProvider), ICommandBarGameManager
{
    protected override async Task InitializeCoreAsync(CancellationToken token)
    {
        Logger?.LogInformation("PCreating command bar components...");

        var contentParser = ServiceProvider.GetRequiredService<IXmlContainerContentParser>();
        contentParser.XmlParseError += OnParseError;

        var parsedCommandBarComponents = new ValueListDictionary<Crc32, CommandBarComponentData>();

        try
        {
            await Task.Run(() => contentParser.ParseEntriesFromContainerXml(
                    "DATA\\XML\\CommandBarComponentFiles.XML",
                    ErrorListener,
                    GameRepository,
                    ".\\DATA\\XML",
                    parsedCommandBarComponents,
                    VerifyFilePathLength),
                token);
        }
        finally
        {
            contentParser.XmlParseError -= OnParseError;
        }
    }

    private void OnParseError(object sender, XmlContainerParserErrorEventArgs e)
    {
        if (e.IsContainer || e.IsError)
        {
            e.Continue = false;
            ErrorListener.OnInitializationError(new InitializationError
            {
                GameManager = ToString(),
                Message = GetMessage(e)
            });
        }
    }

    private static string GetMessage(XmlContainerParserErrorEventArgs errorEventArgs)
    {
        if (errorEventArgs.IsError)
            return $"Error while parsing CommandBar XML file '{errorEventArgs.File}': {errorEventArgs.Exception.Message}";
        return "Could not find CommandBarComponentFiles.xml";
    }

    private void VerifyFilePathLength(string filePath)
    {
        if (filePath.Length > PGConstants.MaxCommandBarDatabaseFileName)
        {
            ErrorListener.OnInitializationError(new InitializationError
            {
                GameManager = ToString(),
                Message = $"CommandBar file '{filePath}' is longer than {PGConstants.MaxCommandBarDatabaseFileName} characters."
            });
        }
    }
}

