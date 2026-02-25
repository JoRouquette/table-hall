using System.Collections.Generic;

namespace TableHall.Dsl.Compilation;

public sealed record CompiledProgram(IReadOnlyDictionary<string, BoundExpr> Outputs);
