using System;
using System.Collections.Generic;
using AET.ModVerify.Reporting;
using AnakinRaW.CommonUtilities;
using PG.StarWarsGame.Engine.IO.Repositories;

namespace AET.ModVerify.Verifiers;

internal abstract class InitializationErrorReporterBase<T>(IGameRepository gameRepository, IServiceProvider serviceProvider)
{
    protected readonly IGameRepository GameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
    protected readonly IServiceProvider ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    public abstract string Name { get; }

    public IEnumerable<VerificationError> GetErrors(IEnumerable<T> errors)
    {
        foreach (var error in errors)
        {
            CreateError(error, out var data);
            yield return new VerificationError(data.Identifier, data.Message, Name, data.Assets, data.Severity);
        }
    }

    protected abstract void CreateError(T error, out ErrorData errorData);

    protected readonly ref struct ErrorData
    {
        public string Identifier { get; }
        public string Message { get; }
        public IEnumerable<string> Assets { get; }
        public VerificationSeverity Severity { get; }

        public ErrorData(string identifier, string message, IEnumerable<string> assets, VerificationSeverity severity)
        {
            ThrowHelper.ThrowIfNullOrEmpty(identifier);
            ThrowHelper.ThrowIfNullOrEmpty(message);
            Identifier = identifier;
            Message = message;
            Assets = assets;
            Severity = severity;
        }

        public ErrorData(string identifier, string message, VerificationSeverity severity) : this(identifier, message, [], severity)
        {
        }
    }
}