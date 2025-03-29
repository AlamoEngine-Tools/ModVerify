using System;
using System.Collections.Generic;
using AnakinRaW.CommonUtilities;
using PG.StarWarsGame.Engine.IO;

namespace AET.ModVerify.Reporting.Reporters.Engine;

internal abstract class EngineErrorReporterBase<T>(IGameRepository gameRepository, IServiceProvider serviceProvider)
{
    protected readonly IGameRepository GameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
    protected readonly IServiceProvider ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    public abstract string Name { get; }

    public IEnumerable<VerificationError> GetErrors(IEnumerable<T> errors)
    {
        foreach (var error in errors)
        {
            var errorData = CreateError(error);
            yield return new VerificationError(
                errorData.Identifier, errorData.Message, [Name], errorData.Context, errorData.Asset, errorData.Severity);
        }
    }

    protected abstract ErrorData CreateError(T error);

    protected readonly ref struct ErrorData
    {
        public string Identifier { get; }
        public string Message { get; }
        public IEnumerable<string> Context { get; }
        public string Asset { get; }
        public VerificationSeverity Severity { get; }

        public ErrorData(string identifier, string message, IEnumerable<string> context, string asset, VerificationSeverity severity)
        {
            ThrowHelper.ThrowIfNullOrEmpty(identifier);
            ThrowHelper.ThrowIfNullOrEmpty(message);
            Identifier = identifier;
            Message = message;
            Context = context;
            Asset = asset;
            Severity = severity;
        }

        public ErrorData(string identifier, string message, string asset, VerificationSeverity severity) : this(identifier, message, [], asset, severity)
        {
        }
    }
}