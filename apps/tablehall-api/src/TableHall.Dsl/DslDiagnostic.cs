namespace TableHall.Dsl;

public sealed record DslDiagnostic(
  string Code,
  string Message,
  DslSeverity Severity,
  string? Path = null,
  string? NodeKind = null
);
