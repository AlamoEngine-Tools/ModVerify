using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PG.StarWarsGame.Engine.ErrorReporting;

public sealed class EngineAssert
{
    private static readonly string ThisNameSpace = typeof(EngineAssert).Namespace!;
    private const string NullLiteral = "NULL";

    public string Value { get; }

    public IReadOnlyCollection<string> Context { get; }

    public string Message { get; }

    public string Method { get; }

    public int MethodOffset { get; }

    public string? TypeName { get; }

    public EngineAssertKind Kind { get; }

    private EngineAssert(EngineAssertKind kind, object? value, IEnumerable<string> context, string? type, string method, int methodOffset, string message)
    {
        Kind = kind;
        Value = value?.ToString() ?? NullLiteral;
        Context = [..context];
        Message = message ?? throw new ArgumentNullException(nameof(message));
        TypeName = type ?? throw new ArgumentNullException(nameof(type));
        Method = method ?? throw new ArgumentNullException(nameof(method));
        MethodOffset = methodOffset;
    }

    internal static EngineAssert FromNullOrEmpty(string? message = null)
    {
        return FromNullOrEmpty([], message);
    }

    internal static EngineAssert FromNullOrEmpty(IEnumerable<string> context, string? message = null)
    {
        return Create(EngineAssertKind.NullOrEmptyValue, null, context, message ?? "Expected value to be not null or empty");
    }

    internal static EngineAssert Create(EngineAssertKind kind, object? value, IEnumerable<string> context, string message)
    {
        var frame = GetCausingFrame(new StackTrace());
        if (frame is null)
            return new EngineAssert(kind, value, context, null, "UNKNOWN SOURCE", -1, message);
        var offset = frame.GetNativeOffset();
        var method = frame.GetMethod();
        var methodInfo = GetMethodInfo(method);
        return new EngineAssert(kind, value, context, methodInfo.type, methodInfo.method, offset, message);
    }

    private static StackFrame? GetCausingFrame(StackTrace trace)
    {
        if (trace.FrameCount == 0)
            return null;
        for (var i = 0; i < trace.FrameCount; i++)
        {
            var frame = trace.GetFrame(i);
            var method = frame.GetMethod();
            if (method.DeclaringType is null || method.DeclaringType.Namespace?.Equals(ThisNameSpace) == false)
                return frame;
        }
        return null;
    }

    private static (string? type, string method) GetMethodInfo(MethodBase method)
    {
        string? type = null;
        if (method.DeclaringType is not null)
            type = method.DeclaringType.FullName;

        var methodName = new StringBuilder();
        methodName.Append(method.Name);
        methodName.Append('(');
        methodName.Append(string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name)));
        methodName.Append(')');
        return (type, methodName.ToString());
    }
}