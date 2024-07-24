using System;
using System.Collections.Generic;
using System.Text;
using AET.ModVerify.Reporting;

namespace AET.ModVerify;

public sealed class GameVerificationException(IEnumerable<VerificationError> errors) : Exception
{
    private readonly string? _error = null;
    private readonly IEnumerable<VerificationError> _errors = errors ?? throw new ArgumentNullException(nameof(errors));

    public GameVerificationException(VerificationError error) : this([error])
    {
    }

    /// <inheritdoc/>
    public override string Message => Error;

    private string Error
    {
        get
        {
            if (_error != null)
                return _error;
            var stringBuilder = new StringBuilder();

            foreach (var error in _errors)
                stringBuilder.AppendLine($"Verification error: {error.Id}:{error.Message};");
            return stringBuilder.ToString().TrimEnd(';');
        }
    }
}