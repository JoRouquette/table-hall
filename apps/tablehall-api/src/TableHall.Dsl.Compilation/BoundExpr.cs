using TableHall.Dsl;

namespace TableHall.Dsl.Compilation;

public interface BoundExpr
{
  DslType Type { get; }
}

public sealed record BoundConst(DslConstValue Value, DslType ConstType) : BoundExpr
{
  public DslType Type => ConstType;
}

public sealed record BoundRef(SymbolId Symbol, DslType RefType) : BoundExpr
{
  public DslType Type => RefType;
}

public sealed record BoundUnary(string Op, BoundExpr Operand, DslType ResultType) : BoundExpr
{
  public DslType Type => ResultType;
}

public sealed record BoundBinary(string Op, BoundExpr Left, BoundExpr Right, DslType ResultType)
  : BoundExpr
{
  public DslType Type => ResultType;
}

public sealed record BoundIf(BoundExpr Cond, BoundExpr Then, BoundExpr Else, DslType ResultType)
  : BoundExpr
{
  public DslType Type => ResultType;
}

public sealed record BoundCall(string Fn, BoundExpr[] Args, DslType ResultType) : BoundExpr
{
  public DslType Type => ResultType;
}

public sealed record BoundAgg(string Op, BoundExpr Source, DslType ResultType) : BoundExpr
{
  public DslType Type => ResultType;
}
