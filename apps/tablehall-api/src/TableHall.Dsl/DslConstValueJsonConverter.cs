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
      throw new JsonException($"{DslDiagnosticCodes.MalformedConst}: constant must be an object");

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
          val = val with { Decimal = ReadDecimalLiteral(prop.Value) };
          break;
        case "bool":
          val = val with { Bool = prop.Value.GetBoolean() };
          break;
        case "string":
          val = val with { String = ReadString(prop.Value) };
          break;
        default:
          throw new JsonException(
            $"{DslDiagnosticCodes.MalformedConst}: unknown constant key '{prop.Name}'"
          );
      }
    }

    if (count != 1)
      throw new JsonException(
        $"{DslDiagnosticCodes.MalformedConst}: a constant must carry exactly one key, found {count}"
      );

    return val;
  }

  private static string ReadDecimalLiteral(JsonElement element)
  {
    if (element.ValueKind != JsonValueKind.String)
      throw new JsonException(
        $"{DslDiagnosticCodes.MalformedConst}: a decimal must be encoded as a string, never as a JSON number"
      );

    var raw = element.GetString();
    if (!DslConstValue.IsLegalDecimalLiteral(raw))
      throw new JsonException(
        $"{DslDiagnosticCodes.MalformedConst}: '{raw}' is not a legal decimal literal"
      );

    var normalised = DslConstValue.NormaliseDecimalLiteral(raw!);
    if (normalised == "-0")
      throw new JsonException(
        $"{DslDiagnosticCodes.MalformedConst}: negative zero is not a legal decimal literal"
      );

    return normalised;
  }

  private static string ReadString(JsonElement element)
  {
    var value =
      element.GetString()
      ?? throw new JsonException($"{DslDiagnosticCodes.MalformedConst}: string must not be null");

    if (value.Length > DslLimits.MaxStringLength)
      throw new JsonException(
        $"{DslDiagnosticCodes.LimitExceeded}: string constant of {value.Length} characters exceeds the limit of {DslLimits.MaxStringLength}"
      );

    return value;
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
      throw new JsonException($"{DslDiagnosticCodes.MalformedConst}: constant carries no value");
    writer.WriteEndObject();
  }
}
