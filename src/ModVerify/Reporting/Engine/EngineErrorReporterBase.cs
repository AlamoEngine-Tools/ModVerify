using System;
using System.Collections.Generic;
using AET.ModVerify.Verifiers;
using AnakinRaW.CommonUtilities;
using PG.StarWarsGame.Engine.IO;

namespace AET.ModVerify.Reporting.Engine;

internal abstract class EngineErrorReporterBase<T> : IGameVerifierInfo
{
    protected readonly IGameRepository GameRepository;
    protected readonly IServiceProvider ServiceProvider;

    public IGameVerifierInfo? Parent => null;

    public IReadOnlyList<IGameVerifierInfo> VerifierChain { get; }

    public string Name => GetType().FullName;

    public abstract string FriendlyName { get; }

    protected EngineErrorReporterBase(IGameRepository gameRepository, IServiceProvider serviceProvider)
    {
        GameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        VerifierChain = [this];
    }

    public IEnumerable<VerificationError> GetErrors(IEnumerable<T> errors)
    {
        foreach (var error in errors)
        {
            var errorData = CreateError(error);
            yield return new VerificationError(
                errorData.Identifier, errorData.Message, this, errorData.Context, errorData.Asset, errorData.Severity);
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
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Asset = asset;
            Severity = severity;
        }

        public ErrorData(string identifier, string message, string asset, VerificationSeverity severity) 
            : this(identifier, message, [], asset, severity)
        {
        }
    }
}