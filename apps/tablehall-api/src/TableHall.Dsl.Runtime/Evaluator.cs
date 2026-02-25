using System;
using System.Collections.Generic;
using TableHall.Dsl;
using TableHall.Dsl.Compilation;

namespace TableHall.Dsl.Runtime;

public static class Evaluator
{
  public static (DslValue? value, List<DslDiagnostic> diagnostics) Evaluate(
    BoundExpr expr,
    IValueProvider provider
  )
  {
    // TODO: implement pure evaluation logic with runtime diagnostics
    throw new NotImplementedException();
  }
}
