using System.Text.Json.Serialization;

namespace TableHall.Dsl;

/// <summary>
/// Expression tree of ABI 1.0.0 section 3. Property names are literal and frozen: they depend on
/// no naming policy, in either direction. Wire order is not significant; canonical order is
/// (section 6).
/// </summary>
[JsonConverter(typeof(ExprJsonConverter))]
public abstract record Expr
{
  [JsonPropertyName("kind")]
  public abstract string Kind { get; }
}

public sealed record ConstExpr([property: JsonPropertyName("value")] DslConstValue Value) : Expr
{
  [JsonPropertyName("kind")]
  public override string Kind => "const";
}

public sealed record RefExpr([property: JsonPropertyName("key")] string Key) : Expr
{
  [JsonPropertyName("kind")]
  public override string Kind => "ref";
}

public sealed record UnaryExpr(
  [property: JsonPropertyName("op")] string Op,
  [property: JsonPropertyName("operand")] Expr Operand
) : Expr
{
  [JsonPropertyName("kind")]
  public override string Kind => "unary";
}

public sealed record BinaryExpr(
  [property: JsonPropertyName("op")] string Op,
  [property: JsonPropertyName("left")] Expr Left,
  [property: JsonPropertyName("right")] Expr Right
) : Expr
{
  [JsonPropertyName("kind")]
  public override string Kind => "binary";
}

public sealed record IfExpr(
  [property: JsonPropertyName("cond")] Expr Cond,
  [property: JsonPropertyName("then")] Expr Then,
  [property: JsonPropertyName("else")] Expr Else
) : Expr
{
  [JsonPropertyName("kind")]
  public override string Kind => "if";
}

public sealed record CallExpr(
  [property: JsonPropertyName("fn")] string Fn,
  [property: JsonPropertyName("args")] Expr[] Args
) : Expr
{
  [JsonPropertyName("kind")]
  public override string Kind => "call";
}

public sealed record AggExpr(
  [property: JsonPropertyName("op")] string Op,
  [property: JsonPropertyName("source")] Expr Source
) : Expr
{
  [JsonPropertyName("kind")]
  public override string Kind => "agg";
}
