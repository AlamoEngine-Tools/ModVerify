using System;
using System.Collections.Generic;
using System.Text;
using AET.ModVerify.Reporting;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO;

namespace AET.ModVerify.Verifiers;

internal sealed class GameAssertErrorReporter(IGameRepository gameRepository, IServiceProvider serviceProvider)
    : InitializationErrorReporterBase<EngineAssert>(gameRepository, serviceProvider)
{
    public override string Name => "GameAsserts";

    protected override ErrorData CreateError(EngineAssert assert)
    {
        var assets = new List<string>
        {
            GetLocation(assert)
        };
        if (assert.Value is not null)
            assets.Add($"value='{assert.Value}'");
        if (assert.Context is not null)
            assets.Add($"context='{assert.Context}'");
        return new ErrorData(GetIdFromError(assert.Kind), assert.Message, assets, VerificationSeverity.Warning);
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