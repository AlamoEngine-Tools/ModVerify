using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AET.ModVerify.Reporting;
using AET.ModVerify.Reporting.Diagnostics;
using Xunit;

namespace ModVerify.Test.Reporting;

public class ErrorDescriptorCatalogTest
{
    private static readonly string CatalogNamespace = typeof(AudioErrors).Namespace!;

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
    {
        return typeof(ErrorDescriptor).Assembly.GetTypes()
            .Where(t => t.IsClass && t.Namespace == CatalogNamespace);
    }

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
}
