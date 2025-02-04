// Copyright (c) Alamo Engine Tools and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers.Primitives;

namespace PG.StarWarsGame.Files.XML;

public static class XmlServiceContribution 
{
    public static void SupportXML(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IPrimitiveXmlParserErrorListener>(_ => new PrimitiveXmlParserErrorBroker());
        serviceCollection.AddSingleton<IPrimitiveXmlErrorParserProvider>(sp => sp.GetRequiredService<IPrimitiveXmlParserErrorListener>());
        serviceCollection.AddSingleton<IPrimitiveParserProvider>(sp => new PrimitiveParserProvider(sp));
    }
}