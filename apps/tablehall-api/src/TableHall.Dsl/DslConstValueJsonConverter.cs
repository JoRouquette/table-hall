using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TableHall.Dsl;

public sealed class DslConstValueJsonConverter : JsonConverter<DslConstValue>
{
  public override DslConstValue? Read(
    ref Utf8JsonReader reader,
    Type typeToConvert,
    JsonSerializerOptions options
  )
  {
    using var doc = JsonDocument.ParseValue(ref reader);
    var root = doc.RootElement;
    if (root.ValueKind != JsonValueKind.Object)
      throw new JsonException("DslConstValue must be an object");
    int count = 0;
    DslConstValue val = new();
    foreach (var prop in root.EnumerateObject())
    {
      count++;
      switch (prop.Name)
      {
        case "int":
          val = val with { Int = prop.Value.GetInt32() };
          break;
        case "decimal":
          val = val with { Decimal = prop.Value.GetString() };
          break;
        case "bool":
          val = val with { Bool = prop.Value.GetBoolean() };
          break;
        case "string":
          val = val with { String = prop.Value.GetString() };
          break;
        default:
          throw new JsonException($"Unknown DslConstValue key: {prop.Name}");
      }
    }
    if (count != 1)
      throw new JsonException("DslConstValue must have exactly one key");
    return val;
  }

  public override void Write(
    Utf8JsonWriter writer,
    DslConstValue value,
    JsonSerializerOptions options
  )
  {
    writer.WriteStartObject();
    if (value.Int is not null)
      writer.WriteNumber("int", value.Int.Value);
    else if (value.Decimal is not null)
      writer.WriteString("decimal", value.Decimal);
    else if (value.Bool is not null)
      writer.WriteBoolean("bool", value.Bool.Value);
    else if (value.String is not null)
      writer.WriteString("string", value.String);
    else
      throw new JsonException("DslConstValue must have a value");
    writer.WriteEndObject();
  }
}
