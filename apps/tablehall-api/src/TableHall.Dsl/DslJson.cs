using System.Text.Json;

namespace TableHall.Dsl;

/// <summary>
/// Conformant entry point for reading and writing an expression tree. Going through
/// <see cref="JsonSerializer"/> directly parses but does not enforce the bounds of section 3.8;
/// this does both.
/// </summary>
public static class DslJson
{
  public static string Serialize(Expr expr) =>
    JsonSerializer.Serialize(expr, DslJsonOptions.Options);

  public static Expr Deserialize(string json)
  {
    var expr =
      JsonSerializer.Deserialize<Expr>(json, DslJsonOptions.Options)
      ?? throw new JsonException($"{DslDiagnosticCodes.MalformedNode}: empty expression");

    DslLimits.Validate(expr);
    return expr;
  }
}
