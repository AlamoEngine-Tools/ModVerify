using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.ErrorReporting;

internal sealed class GameErrorReporterWrapper : XmlErrorReporter, IGameErrorReporter
{
    internal event EventHandler<InitializationError>? InitializationError;

    private readonly IGameErrorReporter? _errorReporter;
    private readonly ILogger? _logger;

    public GameErrorReporterWrapper(IGameErrorReporter? errorReporter, IServiceProvider serviceProvider)
    {
        if (errorReporter is null)
            return;
        _errorReporter = errorReporter;
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    public void Report(XmlError error)
    {
        _errorReporter?.Report(error);
    }

    public void Report(InitializationError error)
    {
        InitializationError?.Invoke(this, error);
        if (_errorReporter is null)
            return;
        _errorReporter.Report(error);
    }

    public void Assert(EngineAssert assert)
    {
        _errorReporter?.Assert(assert);
    }

    public override void Report(IPetroglyphXmlParser parser, XmlParseErrorEventArgs error)
    {
        if (_errorReporter is null)
            return;

        _logger?.LogWarning($"Xml parser '{parser}' reported error: {error.Message}");

        Report(new XmlError
        {
            FileLocation = error.Location,
            Parser = parser,
            Message = error.Message,
            ErrorKind = error.ErrorKind,
            Element = error.Element
        });
    }
}