using System;
using AnakinRaW.CommonUtilities;

namespace RepublicAtWar.DevLauncher.Petroglyph.Verification;

public class VerificationError
{
    public const string GeneralExceptionId = "VE01";

    public string Id { get; }

    public string Message { get; }

    public override string ToString()
    {
        return $"{Id}: {Message}";
    }

    protected VerificationError(string id, string message)
    {
        ThrowHelper.ThrowIfNullOrEmpty(id);
        Id = id;
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    public static VerificationError Create(string id, string message)
    {
        return new VerificationError(id, message);
    }

    public static VerificationError CreateFromException(Exception exception, string failedOperation)
    {
        return Create(GeneralExceptionId, $"Verification of {failedOperation} caused an {exception.GetType().Name}: {exception.Message}");
    }
}

public class VerificationError<T> : VerificationError
{
    public T Context { get; }

    protected VerificationError(string id, string message, T context) : base(id, message)
    {
        Context = context;
    }

    public static VerificationError Create(string id, string message, T context)
    {
        return new VerificationError<T>(id, message, context);
    }
}