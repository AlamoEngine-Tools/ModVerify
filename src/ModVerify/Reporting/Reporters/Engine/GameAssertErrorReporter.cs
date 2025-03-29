using System;
using System.Collections.Generic;
using System.Text;
using AET.ModVerify.Verifiers;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO;

namespace AET.ModVerify.Reporting.Reporters.Engine;

internal sealed class GameAssertErrorReporter(IGameRepository gameRepository, IServiceProvider serviceProvider)
    : EngineErrorReporterBase<EngineAssert>(gameRepository, serviceProvider)
{
    public override string Name => "GameAsserts";

    protected override ErrorData CreateError(EngineAssert assert)
    {
        var context = new List<string>();

        if (assert.Value is not null)
            context.Add($"value='{assert.Value}'");
        if (assert.Context is not null)
            context.Add($"context='{assert.Context}'");

        // The location is the only identifiable thing of an assert. 'Value' might be null, thus we cannot use it. 
        var asset = GetLocation(assert);

        return new ErrorData(GetIdFromError(assert.Kind), assert.Message, asset, VerificationSeverity.Warning);
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
            _ => throw new ArgumentOutOfRangeException(nameof(assertKind), assertKind, null)
        };
    }
}