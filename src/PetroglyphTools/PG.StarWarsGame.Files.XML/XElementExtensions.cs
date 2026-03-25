using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML;

public static class XElementExtensions
{
    extension(XElement element)
    {
        /// <summary>
        /// Gets the value of the element as the Petroglyph engine would parse it.
        /// That is, if the element has child elements, it returns an empty string, otherwise it returns the value of the element.
        /// </summary>
        public string PGValue => element.HasElements ? string.Empty : element.Value;
    }
}