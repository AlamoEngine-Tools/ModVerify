using System;
using System.Collections.Generic;
using System.Text;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO;

namespace AET.ModVerify.Reporting.Engine;

internal sealed class GameAssertErrorReporter(IGameRepository gameRepository, IServiceProvider serviceProvider)
    : EngineErrorReporterBase<EngineAssert>(gameRepository, serviceProvider)
{
    public override string FriendlyName => "Game Engine Asserts";

    protected override ErrorData CreateError(EngineAssert assert)
    {
        var descriptor = GetDescriptor(assert.Kind);
        var context = new List<string>();
        context.AddRange(assert.Context);
        context.Add($"location='{GetLocation(assert)}'");
        return new ErrorData(
            descriptor.Id,
            assert.Message,
            context,
            assert.Value,
            descriptor.Severity);
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

    private static ErrorDescriptor GetDescriptor(EngineAssertKind assertKind)
    {
        return assertKind switch
        {
            EngineAssertKind.NullOrEmptyValue => Diagnostics.Asserts.NullOrEmptyValue,
            EngineAssertKind.ValueOutOfRange => Diagnostics.Asserts.ValueOutOfRange,
            EngineAssertKind.InvalidValue => Diagnostics.Asserts.InvalidValue,
            EngineAssertKind.FileNotFound => Diagnostics.Asserts.FileNotFound,
            EngineAssertKind.DuplicateEntry => Diagnostics.Asserts.DuplicateEntry,
            EngineAssertKind.CorruptBinary => Diagnostics.Asserts.CorruptBinary,
            _ => throw new ArgumentOutOfRangeException(nameof(assertKind), assertKind, null)
        };
    }
}