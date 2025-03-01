using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PG.StarWarsGame.Engine.ErrorReporting;

public sealed class EngineAssert
{
    private static readonly string ThisNameSpace = typeof(EngineAssert).Namespace!;

    public object? Value { get; }

    public object? Context { get; }

    public string Message { get; }

    public string Method { get; }

    public string? TypeName { get; }

    public EngineAssertKind Kind { get; }

    internal EngineAssert(EngineAssertKind kind, object? value, object? context, string? type, string method, string message)
    {
        Kind = kind;
        Value = value;
        Context = context;
        Message = message ?? throw new ArgumentNullException(nameof(message));
        TypeName = type ?? throw new ArgumentNullException(nameof(type));
        Method = method ?? throw new ArgumentNullException(nameof(method));
    }

    internal static EngineAssert FromNullOrEmpty(object? context = null, string? message = null)
    {
        return Create(EngineAssertKind.NullOrEmptyValue, null, context, message ?? "Expected value to be not null or empty");
    }

    internal static EngineAssert Create(EngineAssertKind kind, object? value, object? context, string message)
    {
        var frame = GetCausingFrame(new StackTrace());
        if (frame is null)
            return new EngineAssert(kind, value, context, null, "UNKNOWN SOURCE", message);
        var method = frame.GetMethod();
        var methodInfo = GetMethodInfo(method);
        return new EngineAssert(kind, value, context, methodInfo.type, methodInfo.method, message);
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