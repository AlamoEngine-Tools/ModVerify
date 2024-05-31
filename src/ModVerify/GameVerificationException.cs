using System;
using System.Collections.Generic;
using System.Text;
using AET.ModVerify.Steps;
using AnakinRaW.CommonUtilities.SimplePipeline;

namespace AET.ModVerify;

public sealed class GameVerificationException(IEnumerable<GameVerificationStep> failedSteps) : Exception
{
    private readonly string? _error = null;
    private readonly IEnumerable<IStep> _failedSteps = failedSteps ?? throw new ArgumentNullException(nameof(failedSteps));

    public GameVerificationException(GameVerificationStep step) : this([step])
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

            foreach (var step in _failedSteps)
                stringBuilder.Append($"Verification step '{step}' has errors;");
            return stringBuilder.ToString().TrimEnd(';');
        }
    }
}