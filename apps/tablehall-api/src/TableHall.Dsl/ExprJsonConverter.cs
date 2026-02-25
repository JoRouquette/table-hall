using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TableHall.Dsl;

public sealed class ExprJsonConverter : JsonConverter<Expr>
{
  public override Expr? Read(
    ref Utf8JsonReader reader,
    Type typeToConvert,
    JsonSerializerOptions options
  )
  {
    using var doc = JsonDocument.ParseValue(ref reader);
    var root = doc.RootElement;
    if (!root.TryGetProperty("kind", out var kindProp))
      throw new JsonException("Missing 'kind' discriminator");
    var kind = kindProp.GetString();
    return kind switch
    {
      "const" => JsonSerializer.Deserialize<ConstExpr>(root.GetRawText(), options),
      "ref" => JsonSerializer.Deserialize<RefExpr>(root.GetRawText(), options),
      "unary" => JsonSerializer.Deserialize<UnaryExpr>(root.GetRawText(), options),
      "binary" => JsonSerializer.Deserialize<BinaryExpr>(root.GetRawText(), options),
      "if" => JsonSerializer.Deserialize<IfExpr>(root.GetRawText(), options),
      "call" => JsonSerializer.Deserialize<CallExpr>(root.GetRawText(), options),
      "agg" => JsonSerializer.Deserialize<AggExpr>(root.GetRawText(), options),
      _ => throw new JsonException($"Unknown kind: {kind}"),
    };
  }

  public override void Write(Utf8JsonWriter writer, Expr value, JsonSerializerOptions options)
  {
    switch (value)
    {
      case ConstExpr c:
        JsonSerializer.Serialize(writer, c, options);
        break;
      case RefExpr r:
        JsonSerializer.Serialize(writer, r, options);
        break;
      case UnaryExpr u:
        JsonSerializer.Serialize(writer, u, options);
        break;
      case BinaryExpr b:
        JsonSerializer.Serialize(writer, b, options);
        break;
      case IfExpr i:
        JsonSerializer.Serialize(writer, i, options);
        break;
      case CallExpr call:
        JsonSerializer.Serialize(writer, call, options);
        break;
      case AggExpr agg:
        JsonSerializer.Serialize(writer, agg, options);
        break;
      default:
        throw new JsonException($"Unknown Expr type: {value.GetType().Name}");
    }
  }
}
