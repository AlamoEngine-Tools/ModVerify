using AET.ModVerify.Settings;
using PG.StarWarsGame.Engine;
using System;
using System.Collections.Generic;
using System.Threading;
using AET.ModVerify.Reporting.Diagnostics;

namespace AET.ModVerify.Verifiers.Commons;

public sealed class DuplicateVerifier : GameVerifier<IDuplicateVerificationContext>
{
    public override string FriendlyName => "Duplicate Verifier";

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

    public override void Verify(IDuplicateVerificationContext toVerify, IReadOnlyCollection<string> contextInfo, CancellationToken token)
    {
        foreach (var crc32 in toVerify.GetCrcs())
        {
            if (toVerify.HasDuplicates(crc32, out var entryNames, out var context, out var errorMessage))
            {
                AddError(CommonErrors.Duplicate(this, entryNames, errorMessage, context));
            }
        }
    }
}