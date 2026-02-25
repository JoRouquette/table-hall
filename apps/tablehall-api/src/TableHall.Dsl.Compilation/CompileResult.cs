using System.Collections.Generic;

namespace TableHall.Dsl.Compilation;

public sealed record CompileResult(
  CompiledProgram? Program,
  IReadOnlyList<TableHall.Dsl.DslDiagnostic> Diagnostics
);
