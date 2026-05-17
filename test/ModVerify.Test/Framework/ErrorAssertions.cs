using System.Collections.Generic;
using System.Linq;
using AET.ModVerify.Reporting;
using Xunit;

namespace ModVerify.Test.Framework;

internal static class ErrorAssertions
{
    public static VerificationError Single(
        IEnumerable<VerificationError> errors, string id,
        string? asset = null, string? contextContains = null)
    {
        var matches = errors.Where(e => Match(e, id, asset, contextContains)).ToList();
        Assert.Single(matches);
        return matches[0];
    }

    public static void None(IEnumerable<VerificationError> errors, string id)
    {
        Assert.DoesNotContain(errors, e => e.Id == id);
    }

    public static void Exactly(IEnumerable<VerificationError> errors, int expected, string id)
    {
        Assert.Equal(expected, errors.Count(e => e.Id == id));
    }

    private static bool Match(VerificationError e, string id, string? asset, string? contextContains)
    {
        if (e.Id != id)
            return false;
        if (asset != null && e.Asset != asset)
            return false;
        return contextContains == null || e.ContextEntries.Any(c => c.Contains(contextContains));
    }
}
