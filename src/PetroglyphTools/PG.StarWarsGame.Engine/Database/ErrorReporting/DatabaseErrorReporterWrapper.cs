using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Engine.Database.ErrorReporting;

internal sealed class DatabaseErrorReporterWrapper : XmlErrorReporter, IDatabaseErrorReporter
{
    internal event EventHandler<InitializationError>? InitializationError;

    private readonly IDatabaseErrorReporter? _errorListener;
    private readonly ILogger? _logger;

    public DatabaseErrorReporterWrapper(IDatabaseErrorReporter? errorListener, IServiceProvider serviceProvider)
    {
        if (errorListener is null)
            return;
        _errorListener = errorListener;
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    public void Report(XmlError error)
    {
        _errorListener?.Report(error);
    }

    public void Report(InitializationError error)
    {
        InitializationError?.Invoke(this, error);
        if (_errorListener is null)
            return;
        _errorListener.Report(error);
    }

    public override void Report(string parser, XmlParseErrorEventArgs error)
    {
        if (_errorListener is null)
            return;

        _logger?.LogWarning($"Xml parser '{parser}' reported error: {error}");

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