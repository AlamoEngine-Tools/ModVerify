using System;
using System.Collections.Generic;
using System.Text;
using AET.ModVerify.Reporting;

namespace AET.ModVerify;

public sealed class GameVerificationException : Exception
{
    private readonly string? _errorMessage = null;
    
    public IReadOnlyCollection<VerificationError> Errors { get; }
    
    private string ErrorMessage
    {
        get
        {
            if (_errorMessage != null)
                return _errorMessage;
            var stringBuilder = new StringBuilder();

            foreach (var error in Errors)
                stringBuilder.AppendLine($"Verification error: {error.Id}: {error.Message};");
            return stringBuilder.ToString().TrimEnd(';');
        }
    }

    /// <inheritdoc/>
    public override string Message => ErrorMessage;

    public GameVerificationException(VerificationError error) : this([error])
    {
    }

    public GameVerificationException(IEnumerable<VerificationError> errors)
    {
        if (errors is null)
            throw new ArgumentNullException(nameof(errors));
        Errors = [..errors];
    }
}