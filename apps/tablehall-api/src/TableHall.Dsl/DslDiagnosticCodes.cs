namespace TableHall.Dsl;

/// <summary>
/// Diagnostic codes of ABI 1.0.0 section 10 that the serialisation layer can already raise.
/// These strings are ABI identifiers: never rename, never recycle. Compilation and evaluation
/// codes arrive with the compiler and the evaluator.
/// </summary>
public static class DslDiagnosticCodes
{
  public const string MalformedNode = "DSL_MALFORMED_NODE";
  public const string MalformedConst = "DSL_MALFORMED_CONST";
  public const string LimitExceeded = "DSL_LIMIT_EXCEEDED";
}
