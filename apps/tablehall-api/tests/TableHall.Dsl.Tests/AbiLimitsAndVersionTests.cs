using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using TableHall.Dsl;
using TableHall.Dsl.Compilation;
using Xunit;

namespace TableHall.Dsl.Tests;

/// <summary>
/// Bounds of ABI 1.0.0 section 3.8 and version handling of section 1.
/// </summary>
public class AbiLimitsAndVersionTests
{
  private static Expr Nest(int depth)
  {
    Expr node = new ConstExpr(DslConstValue.FromInt(1));
    for (var i = 1; i < depth; i++)
    {
      node = new UnaryExpr("-", node);
    }
    return node;
  }

  // ---------------------------------------------------------------- E-05

  [Fact]
  public void ATreeAtTheDepthLimit_IsAccepted()
  {
    DslLimits.Validate(Nest(DslLimits.MaxDepth));
  }

  [Fact]
  public void ATreeDeeperThanTheLimit_IsRejected()
  {
    var error = Assert.ThrowsAny<JsonException>(() =>
      DslLimits.Validate(Nest(DslLimits.MaxDepth + 1))
    );

    Assert.Contains(DslDiagnosticCodes.LimitExceeded, error.Message, StringComparison.Ordinal);
  }

  [Fact]
  public void ACallWithTooManyArguments_IsRejected()
  {
    var args = Enumerable
      .Range(0, DslLimits.MaxCallArguments + 1)
      .Select(i => (Expr)new ConstExpr(DslConstValue.FromInt(i)))
      .ToArray();

    var error = Assert.ThrowsAny<JsonException>(() =>
      DslLimits.Validate(new CallExpr("lookup", args))
    );

    Assert.Contains(DslDiagnosticCodes.LimitExceeded, error.Message, StringComparison.Ordinal);
  }

  [Fact]
  public void AnOverlongReferenceKey_IsRejected()
  {
    var key = new string('a', DslLimits.MaxRefKeyLength + 1);

    var error = Assert.ThrowsAny<JsonException>(() => DslLimits.Validate(new RefExpr(key)));

    Assert.Contains(DslDiagnosticCodes.LimitExceeded, error.Message, StringComparison.Ordinal);
  }

  [Fact]
  public void AnOverlongStringConstant_IsRejectedOnRead()
  {
    var json =
      "{\"kind\":\"const\",\"value\":{\"string\":\""
      + new string('x', DslLimits.MaxStringLength + 1)
      + "\"}}";

    var error = Assert.ThrowsAny<JsonException>(() => DslJson.Deserialize(json));

    Assert.Contains(DslDiagnosticCodes.LimitExceeded, error.Message, StringComparison.Ordinal);
  }

  [Fact]
  public void TooManyNodes_AreRejected()
  {
    // A balanced binary tree of depth 13 holds 8191 nodes, above the 4096 ceiling, while staying
    // well inside the depth limit — so this exercises the node count and nothing else.
    Expr node = new ConstExpr(DslConstValue.FromInt(1));
    for (var i = 0; i < 13; i++)
    {
      node = new BinaryExpr("+", node, node);
    }

    var error = Assert.ThrowsAny<JsonException>(() => DslLimits.Validate(node));

    Assert.Contains(DslDiagnosticCodes.LimitExceeded, error.Message, StringComparison.Ordinal);
  }

  [Fact]
  public void TheJsonReaderStopsBeforeTheStackDoes()
  {
    var json = string.Concat(
      Enumerable.Repeat("{\"kind\":\"unary\",\"op\":\"-\",\"operand\":", 5000)
    );

    // The point is that this fails as an exception rather than tearing the process down.
    Assert.ThrowsAny<Exception>(() => DslJson.Deserialize(json));
  }

  // ---------------------------------------------------------------- I-11

  [Fact]
  public void TheCurrentAbiVersion_IsReadable()
  {
    DslAbi.EnsureReadable(DslAbi.Version);
    DslAbi.EnsureReadable("1.7.3");
  }

  [Fact]
  public void I11_AHigherMajorAbiVersion_IsRefusedExplicitly()
  {
    var error = Assert.ThrowsAny<JsonException>(() => DslAbi.EnsureReadable("2.0.0"));

    Assert.Contains("2.0.0", error.Message, StringComparison.Ordinal);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData("one.zero")]
  public void AnUnreadableAbiVersion_IsRefused(string version)
  {
    Assert.ThrowsAny<JsonException>(() => DslAbi.EnsureReadable(version));
  }

  // ---------------------------------------------------------------- E-07

  [Fact]
  public void AProfileCarriesTheAbiVersion_AndChecksIt()
  {
    var profile = new DslProfileDefinition
    {
      AllowedBinaryOps = new[] { "+" },
      AllowedUnaryOps = new[] { "-" },
      BuiltinFunctions = new[] { "toInt" },
      RoundingModes = new[] { "Floor" },
      UserFunctions = new List<DslProfileDefinition.UserFunction>(),
    };

    Assert.Equal(DslAbi.Version, profile.AbiVersion);
    profile.EnsureReadable();

    var fromTheFuture = profile with { AbiVersion = "2.0.0" };
    Assert.ThrowsAny<JsonException>(() => fromTheFuture.EnsureReadable());
  }
}
