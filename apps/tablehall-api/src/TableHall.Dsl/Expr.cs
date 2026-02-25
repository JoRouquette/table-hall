using System.Text.Json.Serialization;

namespace TableHall.Dsl;

[JsonConverter(typeof(ExprJsonConverter))]
public abstract record Expr
{
  public abstract string Kind { get; }
}

public sealed record ConstExpr(DslConstValue Value) : Expr
{
  public override string Kind => "const";
}

public sealed record RefExpr(string Key) : Expr
{
  public override string Kind => "ref";
}

public sealed record UnaryExpr(string Op, Expr Operand) : Expr
{
  public override string Kind => "unary";
}

public sealed record BinaryExpr(string Op, Expr Left, Expr Right) : Expr
{
  public override string Kind => "binary";
}

public sealed record IfExpr(Expr Cond, Expr Then, Expr Else) : Expr
{
  public override string Kind => "if";
}

public sealed record CallExpr(string Fn, Expr[] Args) : Expr
{
  public override string Kind => "call";
}

public sealed record AggExpr(string Op, Expr Source) : Expr
{
  public override string Kind => "agg";
}
