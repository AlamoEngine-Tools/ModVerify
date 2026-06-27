using System;
using System.Collections.Generic;
using System.Text;
using AET.ModVerify.Reporting;

namespace AET.ModVerify;

/// <summary>
/// The exception that is thrown by a game verifier when one or more verification errors occur during the verification process.
/// </summary>
public sealed class GameVerificationException : Exception
{
    /// <inheritdoc/>
    public override string Message => ErrorMessage;

    /// <summary>
    /// Gets the verification errors that caused this exception to be thrown.
    /// </summary>
    public IReadOnlyCollection<VerificationError> Errors { get; }

    private string ErrorMessage
    {
        get
        {
            if (field != null)
                return field;
            var stringBuilder = new StringBuilder();

            foreach (var error in Errors)
                stringBuilder.AppendLine($"Verification error: {error.Id}: {error.Message};");
            return stringBuilder.ToString().TrimEnd(';');
        }
    } = null;

    internal GameVerificationException(VerificationError error) : this([error])
    {
    }

    internal GameVerificationException(IEnumerable<VerificationError> errors)
    {
        if (errors is null)
            throw new ArgumentNullException(nameof(errors));
        Errors = [..errors];
    }
}