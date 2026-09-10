using System.Text.Json;

namespace TableHall.Dsl;

/// <summary>
/// Bounds of ABI 1.0.0 section 3.8. A contract without bounds is an open door: a reader must
/// refuse an oversized tree rather than try to honour it.
/// </summary>
public static class DslLimits
{
  public const int MaxDepth = 64;
  public const int MaxNodes = 4096;
  public const int MaxCallArguments = 8;
  public const int MaxRefKeyLength = 256;
  public const int MaxStringLength = 4096;

  /// <summary>
  /// Depth ceiling handed to the JSON reader. Higher than the contract's own limit on purpose:
  /// the parser must stop before the stack does, and the contract limit is then applied to the
  /// parsed tree, where it can be reported honestly. One JSON level does not map to one AST level.
  /// </summary>
  public const int JsonReaderMaxDepth = 200;

  public static void Validate(Expr expr)
  {
    var nodes = 0;
    Walk(expr, 1, ref nodes);
  }

  private static void Walk(Expr expr, int depth, ref int nodes)
  {
    if (depth > MaxDepth)
      throw new JsonException(
        $"{DslDiagnosticCodes.LimitExceeded}: nesting deeper than {MaxDepth} levels"
      );

    if (++nodes > MaxNodes)
      throw new JsonException(
        $"{DslDiagnosticCodes.LimitExceeded}: more than {MaxNodes} nodes in a single expression"
      );

    switch (expr)
    {
      case ConstExpr:
        break;

      case RefExpr r:
        if (r.Key.Length > MaxRefKeyLength)
          throw new JsonException(
            $"{DslDiagnosticCodes.LimitExceeded}: reference key of {r.Key.Length} characters exceeds the limit of {MaxRefKeyLength}"
          );
        break;

      case UnaryExpr u:
        Walk(u.Operand, depth + 1, ref nodes);
        break;

      case BinaryExpr b:
        Walk(b.Left, depth + 1, ref nodes);
        Walk(b.Right, depth + 1, ref nodes);
        break;

      case IfExpr i:
        Walk(i.Cond, depth + 1, ref nodes);
        Walk(i.Then, depth + 1, ref nodes);
        Walk(i.Else, depth + 1, ref nodes);
        break;

      case CallExpr c:
        if (c.Args.Length > MaxCallArguments)
          throw new JsonException(
            $"{DslDiagnosticCodes.LimitExceeded}: call to '{c.Fn}' has {c.Args.Length} arguments, the limit is {MaxCallArguments}"
          );
        foreach (var arg in c.Args)
          Walk(arg, depth + 1, ref nodes);
        break;

      case AggExpr a:
        Walk(a.Source, depth + 1, ref nodes);
        break;

      default:
        throw new JsonException(
          $"{DslDiagnosticCodes.MalformedNode}: unknown node type {expr.GetType().Name}"
        );
    }
  }
}
