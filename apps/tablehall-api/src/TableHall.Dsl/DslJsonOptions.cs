using System.Text.Json;

namespace TableHall.Dsl;

public static class DslJsonOptions
{
  public static readonly JsonSerializerOptions Options = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Converters = { new ExprJsonConverter(), new DslConstValueJsonConverter() },
    WriteIndented = false,
  };
}
