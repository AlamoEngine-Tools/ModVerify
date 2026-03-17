using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using PG.StarWarsGame.Engine;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AET.ModVerify.Verifiers.Commons;

public sealed class DuplicateVerifier : GameVerifier<IDuplicateVerificationContext>
{
    public DuplicateVerifier(GameVerifierBase parent) : base(parent)
    {
    }

    public DuplicateVerifier(
        IGameVerifierInfo? parent,
        IStarWarsGameEngine gameEngine,
        GameVerifySettings settings,
        IServiceProvider serviceProvider) 
        : base(parent, gameEngine, settings, serviceProvider)
    {
    }

    public override string FriendlyName => "Duplicates";

    public override void Verify(IDuplicateVerificationContext toVerify, IReadOnlyCollection<string> contextInfo, CancellationToken token)
    {
        foreach (var crc32 in toVerify.GetCrcs())
        {
            if (toVerify.HasDuplicates(crc32, out var entryNames, out var context, out var errorMessage))
            {
                AddError(VerificationError.Create(
                    this,
                    VerifierErrorCodes.Duplicate,
                    errorMessage,
                    VerificationSeverity.Error,
                    context,
                    entryNames));
            }
        }
    }
}