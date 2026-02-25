using System.Collections.Generic;
using TableHall.Dsl;

namespace TableHall.Dsl.Compilation;

public sealed record DslProfileDefinition
{
  public required string[] AllowedBinaryOps { get; init; }
  public required string[] AllowedUnaryOps { get; init; }
  public required string[] BuiltinFunctions { get; init; }
  public required List<UserFunction> UserFunctions { get; init; }
  public required string[] RoundingModes { get; init; }

  public sealed record UserFunction(
    string Key,
    string[] Parameters,
    DslType ReturnType,
    Expr BodyExpr
  );
}
