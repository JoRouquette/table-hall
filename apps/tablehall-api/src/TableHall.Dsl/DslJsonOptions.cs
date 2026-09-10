using System.Text.Json;

namespace TableHall.Dsl;

public static class DslJsonOptions
{
  public static readonly JsonSerializerOptions Options = new()
  {
    // Property names are pinned by [JsonPropertyName] on every node, so this policy no longer
    // decides anything on the wire. It is kept for the surrounding payloads.
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Converters = { new ExprJsonConverter(), new DslConstValueJsonConverter() },
    WriteIndented = false,
    MaxDepth = DslLimits.JsonReaderMaxDepth,
  };
}
