using System.Collections.Generic;
using TableHall.Dsl;

namespace TableHall.Dsl.Compilation;

/// <summary>
/// A language profile: what a template author is allowed to write. A published profile version is
/// immutable, and every published template version pins the one that validated it (EF-025).
/// </summary>
public sealed record DslProfileDefinition
{
  /// <summary>
  /// ABI version this profile was written against. A profile is a durable payload, so it carries
  /// the version (contract section 1). Defaulted rather than required so that in-code profiles
  /// stay terse.
  /// </summary>
  public string AbiVersion { get; init; } = DslAbi.Version;

  public required string[] AllowedBinaryOps { get; init; }
  public required string[] AllowedUnaryOps { get; init; }
  public required string[] BuiltinFunctions { get; init; }
  public required List<UserFunction> UserFunctions { get; init; }
  public required string[] RoundingModes { get; init; }

  /// <summary>
  /// Throws if this profile was written against an ABI major version this build cannot read.
  /// </summary>
  public void EnsureReadable() => DslAbi.EnsureReadable(AbiVersion);

  public sealed record UserFunction(
    string Key,
    string[] Parameters,
    DslType ReturnType,
    Expr BodyExpr
  );
}
