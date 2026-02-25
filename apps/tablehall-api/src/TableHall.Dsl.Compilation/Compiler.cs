using System;
using System.Collections.Generic;
using TableHall.Dsl;

namespace TableHall.Dsl.Compilation;

public static class Compiler
{
  public static CompileResult CompileMany(
    IReadOnlyDictionary<string, Expr> outputs,
    DslProfileDefinition profile,
    ISymbolResolver resolver
  )
  {
    // TODO: implement binding, type-check, topo sort, cycle detection, diagnostics
    throw new NotImplementedException();
  }
}
