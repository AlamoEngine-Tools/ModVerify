using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PG.StarWarsGame.Engine.ErrorReporting;

public sealed class EngineAssert(string source, string message)
{
    public string Message { get; } = message ?? throw new ArgumentNullException(nameof(message));

    public string Source { get; } = source ?? throw new ArgumentNullException(nameof(source));

    public static EngineAssert CreateCapture(string message)
    {
        var stackTrace = new StackTrace();
        var frame = stackTrace.GetFrame(1);
        if (frame is null)
            return new EngineAssert("UNKNOWN SOURCE", message);
        var method = frame.GetMethod();
        var methodName = CreateMethodName(method);
        return new EngineAssert(methodName, message);
    }

    private static string CreateMethodName(MethodBase method)
    {
        var methodName = new StringBuilder();
        if (method.DeclaringType is not null)
            methodName.Append(method.DeclaringType.FullName);
        methodName.Append("::");
        methodName.Append(method.Name);
        methodName.Append('(');
        methodName.Append(string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name)));
        methodName.Append(')');
        return methodName.ToString();
    }
}