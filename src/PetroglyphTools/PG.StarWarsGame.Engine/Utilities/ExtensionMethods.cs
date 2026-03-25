using System.Collections.Generic;

namespace PG.StarWarsGame.Engine.Utilities;

internal static class ExtensionMethods
{
    extension<T>(List<T> list)
    {
        public void ClearAddRange(IEnumerable<T> items)
        {
            list.Clear();
            list.AddRange(items);   
        }
    }
}