using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AET.ModVerify.Reporting;
using AET.ModVerify.Verifiers;
using Xunit;

namespace ModVerify.Test.Reporting;

public class ErrorDescriptorCatalogTest
{
    private const string CatalogNamespace = "AET.ModVerify.Reporting.Diagnostics";

    private static readonly Regex IdFormat = new("^[A-Z]+[0-9]+$");

    private static IReadOnlyList<ErrorDescriptor> AllDescriptors()
    {
        var result = new List<ErrorDescriptor>();
        foreach (var type in CatalogTypes())
        {
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                if (field.FieldType == typeof(ErrorDescriptor))
                    result.Add((ErrorDescriptor)field.GetValue(null)!);
            }
        }
        return result;
    }

    private static IEnumerable<Type> CatalogTypes()
        => typeof(ErrorDescriptor).Assembly.GetTypes().Where(t => t.IsClass && t.Namespace == CatalogNamespace);

    [Fact]
    public void Catalog_IsNotEmpty()
    {
        Assert.NotEmpty(AllDescriptors());
    }

    [Fact]
    public void Catalog_NamesAreUnique()
    {
        var duplicates = AllDescriptors()
            .GroupBy(d => d.Name, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void Catalog_IdsAreWellFormed()
    {
        foreach (var descriptor in AllDescriptors())
            Assert.True(IdFormat.IsMatch(descriptor.Id), $"Descriptor '{descriptor.Name}' has a malformed id '{descriptor.Id}'.");
    }

    [Fact]
    public void Catalog_CategoriesAreNonEmpty()
    {
        foreach (var descriptor in AllDescriptors())
            Assert.False(string.IsNullOrWhiteSpace(descriptor.Category), $"Descriptor '{descriptor.Name}' has an empty category.");
    }

    // Verifies every catalog factory wires to a sensible descriptor and produces a well-formed error.
    // This is the guard for the factory -> descriptor binding, which the compiler cannot check.
    [Fact]
    public void Catalog_FactoriesProduceWellFormedErrors()
    {
        var factories = CatalogTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .Where(m => m.ReturnType == typeof(VerificationError))
            .ToList();

        Assert.NotEmpty(factories);

        foreach (var factory in factories)
        {
            var args = factory.GetParameters().Select(p => SynthesizeArgument(p.ParameterType)).ToArray();
            var label = $"{factory.DeclaringType!.Name}.{factory.Name}";

            var error = (VerificationError)factory.Invoke(null, args)!;

            Assert.True(error is not null, $"{label} returned null.");
            Assert.True(IdFormat.IsMatch(error!.Id), $"{label} produced a malformed id '{error.Id}'.");
            Assert.True(Enum.IsDefined(typeof(VerificationSeverity), error.Severity), $"{label} produced an undefined severity.");
            Assert.False(string.IsNullOrEmpty(error.Asset), $"{label} produced an empty asset.");
            Assert.False(string.IsNullOrEmpty(error.Message), $"{label} produced an empty message.");
        }
    }

    private static object SynthesizeArgument(Type type)
    {
        if (type == typeof(IGameVerifierInfo))
            return StubVerifier.Instance;
        if (type == typeof(string))
            return "value";
        if (type == typeof(int))
            return 0;
        if (typeof(IEnumerable<string>).IsAssignableFrom(type))
            return Array.Empty<string>();
        if (type == typeof(object))
            return "value";
        return type.IsValueType ? Activator.CreateInstance(type)! : "value";
    }

    private sealed class StubVerifier : IGameVerifierInfo
    {
        public static readonly StubVerifier Instance = new();
        public IGameVerifierInfo? Parent => null;
        public IReadOnlyList<IGameVerifierInfo> VerifierChain => [this];
        public string Name => "Stub";
        public string FriendlyName => "Stub";
    }
}
