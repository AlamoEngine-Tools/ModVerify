using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace AET.ModVerify.Reporting.Json;

internal class JsonSuppressionList
{
    [JsonPropertyName("suppressions")]
    public ICollection<JsonSuppressionFilter> Filters { get; }

    public JsonSuppressionList(IEnumerable<SuppressionFilter> suppressionList)
    {
        Filters = suppressionList.Select(x => new JsonSuppressionFilter(x)).ToList();
    }
}