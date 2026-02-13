using System;
using System.Collections.Generic;
using System.Text;
using AET.ModVerify.Verifiers;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO;

namespace AET.ModVerify.Reporting.Engine;

internal sealed class GameAssertErrorReporter(IGameRepository gameRepository, IServiceProvider serviceProvider)
    : EngineErrorReporterBase<EngineAssert>(gameRepository, serviceProvider)
{
    public override string Name => "GameAsserts";

    protected override ErrorData CreateError(EngineAssert assert)
    {
        var context = new List<string>();
        context.AddRange(assert.Context);
        context.Add($"location='{GetLocation(assert)}'");
        return new ErrorData(GetIdFromError(assert.Kind), assert.Message, context, assert.Value, VerificationSeverity.Warning);
    }

    private static string GetLocation(EngineAssert assert)
    {
        var sb = new StringBuilder("method='");
        if (assert.TypeName is not null)
        {
            sb.Append(assert.TypeName);
            sb.Append("::");
        }
        sb.Append(assert.Method);
        sb.Append('\'');
        return sb.ToString();
    }

    private static string GetIdFromError(EngineAssertKind assertKind)
    {
        return assertKind switch
        {
            EngineAssertKind.NullOrEmptyValue => VerifierErrorCodes.AssertValueNullOrEmpty,
            EngineAssertKind.ValueOutOfRange => VerifierErrorCodes.AssertValueOutOfRange,
            EngineAssertKind.InvalidValue => VerifierErrorCodes.AssertValueInvalid,
            EngineAssertKind.FileNotFound => VerifierErrorCodes.FileNotFound,
            _ => throw new ArgumentOutOfRangeException(nameof(assertKind), assertKind, null)
        };
    }
}