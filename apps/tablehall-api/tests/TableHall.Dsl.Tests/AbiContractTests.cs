using System;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using TableHall.Dsl;
using Xunit;

namespace TableHall.Dsl.Tests;

/// <summary>
/// Invariants of the frozen ABI contract 1.0.0.
/// Each test maps to a numbered invariant of the contract; do not rename without amending it.
/// </summary>
public class AbiContractTests
{
  private static Expr SampleTree() =>
    new IfExpr(
      new BinaryExpr(">=", new RefExpr("abilities.str"), new ConstExpr(DslConstValue.FromInt(10))),
      new CallExpr(
        "toInt",
        new Expr[]
        {
          new BinaryExpr(
            "/",
            new UnaryExpr("-", new RefExpr("level")),
            new ConstExpr(DslConstValue.FromDecimal(2.5m))
          ),
          new ConstExpr(DslConstValue.FromString("Floor")),
        }
      ),
      new AggExpr("sum", new RefExpr("inventory.carriedWeight"))
    );

  private static string ConstDecimalJson(string literal) =>
    "{\"kind\":\"const\",\"value\":{\"decimal\":\"" + literal + "\"}}";

  private static T WithCulture<T>(string culture, Func<T> action)
  {
    var previous = Thread.CurrentThread.CurrentCulture;
    try
    {
      Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
      return action();
    }
    finally
    {
      Thread.CurrentThread.CurrentCulture = previous;
    }
  }

  // ---------------------------------------------------------------- I-01

  [Theory]
  [InlineData("const")]
  [InlineData("ref")]
  [InlineData("unary")]
  [InlineData("binary")]
  [InlineData("if")]
  [InlineData("call")]
  [InlineData("agg")]
  public void I01_EveryNodeKind_SurvivesARoundTrip(string kind)
  {
    Expr node = kind switch
    {
      "const" => new ConstExpr(DslConstValue.FromInt(7)),
      "ref" => new RefExpr("level"),
      "unary" => new UnaryExpr("-", new ConstExpr(DslConstValue.FromInt(1))),
      "binary" => new BinaryExpr(
        "+",
        new ConstExpr(DslConstValue.FromInt(1)),
        new ConstExpr(DslConstValue.FromInt(2))
      ),
      "if" => new IfExpr(
        new ConstExpr(DslConstValue.FromBool(true)),
        new ConstExpr(DslConstValue.FromInt(1)),
        new ConstExpr(DslConstValue.FromInt(2))
      ),
      "call" => new CallExpr("toDecimal", new Expr[] { new ConstExpr(DslConstValue.FromInt(3)) }),
      "agg" => new AggExpr("count", new RefExpr("inventory.carriedWeight")),
      _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    var json = JsonSerializer.Serialize(node, DslJsonOptions.Options);
    var back = JsonSerializer.Deserialize<Expr>(json, DslJsonOptions.Options);

    Assert.NotNull(back);
    Assert.Equal(kind, back!.Kind);
    Assert.Equal(
      CanonicalJson.ComputeCanonicalSha256(node),
      CanonicalJson.ComputeCanonicalSha256(back)
    );
  }

  [Fact]
  public void I01_NestedTree_SurvivesARoundTrip()
  {
    var tree = SampleTree();
    var json = JsonSerializer.Serialize(tree, DslJsonOptions.Options);
    var back = JsonSerializer.Deserialize<Expr>(json, DslJsonOptions.Options);

    Assert.NotNull(back);
    Assert.Equal(
      CanonicalJson.ComputeCanonicalSha256(tree),
      CanonicalJson.ComputeCanonicalSha256(back!)
    );
  }

  // ---------------------------------------------------------------- I-02

  [Fact]
  public void I02_EqualTrees_ShareTheSameFingerprint()
  {
    Assert.Equal(
      CanonicalJson.ComputeCanonicalSha256(SampleTree()),
      CanonicalJson.ComputeCanonicalSha256(SampleTree())
    );
  }

  [Fact]
  public void I02_ASingleLiteralApart_ChangesTheFingerprint()
  {
    var a = new BinaryExpr(
      "+",
      new ConstExpr(DslConstValue.FromInt(1)),
      new ConstExpr(DslConstValue.FromInt(2))
    );
    var b = new BinaryExpr(
      "+",
      new ConstExpr(DslConstValue.FromInt(1)),
      new ConstExpr(DslConstValue.FromInt(3))
    );

    Assert.NotEqual(
      CanonicalJson.ComputeCanonicalSha256(a),
      CanonicalJson.ComputeCanonicalSha256(b)
    );
  }

  [Fact]
  public void I02_Fingerprint_IsLowercaseHexOf64Chars()
  {
    var hash = CanonicalJson.ComputeCanonicalSha256(SampleTree());

    Assert.Equal(64, hash.Length);
    Assert.Matches("^[0-9a-f]{64}$", hash);
  }

  // ---------------------------------------------------------------- I-03

  [Theory]
  [InlineData("fr-FR")]
  [InlineData("tr-TR")]
  [InlineData("de-DE")]
  public void I03_Fingerprint_DoesNotDependOnTheCulture(string culture)
  {
    var invariant = CanonicalJson.ComputeCanonicalSha256(SampleTree());
    var underCulture = WithCulture(
      culture,
      () => CanonicalJson.ComputeCanonicalSha256(SampleTree())
    );

    Assert.Equal(invariant, underCulture);
  }

  [Fact]
  public void I03_Fingerprint_DoesNotDependOnTheOrderOfReceivedProperties()
  {
    const string ordered =
      """{"kind":"binary","op":"+","left":{"kind":"const","value":{"int":1}},"right":{"kind":"const","value":{"int":2}}}""";
    const string shuffled =
      """{"right":{"value":{"int":2},"kind":"const"},"left":{"value":{"int":1},"kind":"const"},"op":"+","kind":"binary"}""";

    var a = JsonSerializer.Deserialize<Expr>(ordered, DslJsonOptions.Options)!;
    var b = JsonSerializer.Deserialize<Expr>(shuffled, DslJsonOptions.Options)!;

    Assert.Equal(CanonicalJson.ComputeCanonicalSha256(a), CanonicalJson.ComputeCanonicalSha256(b));
  }

  [Fact]
  public void I03_CanonicalForm_OrdersRefKeysOrdinally_KeyBeforeKind()
  {
    var canonical = CanonicalJson.SerializeCanonical(new RefExpr("level"));

    Assert.Equal("""{"key":"level","kind":"ref"}""", canonical);
  }

  [Fact]
  public void I03_CanonicalForm_DoesNotEscapeNonAsciiCharacters()
  {
    var canonical = CanonicalJson.SerializeCanonical(
      new ConstExpr(DslConstValue.FromString("Épée à deux mains"))
    );

    Assert.Equal("""{"kind":"const","value":{"string":"Épée à deux mains"}}""", canonical);
  }

  // ---------------------------------------------------------------- I-04

  [Fact]
  public void I04_TwoWritingsOfTheSameDecimal_ShareTheSameFingerprint()
  {
    var written = new ConstExpr(DslConstValue.FromDecimal(12.50m));
    var trimmed = new ConstExpr(DslConstValue.FromDecimal(12.5m));

    Assert.Equal(
      CanonicalJson.ComputeCanonicalSha256(trimmed),
      CanonicalJson.ComputeCanonicalSha256(written)
    );
  }

  [Theory]
  [InlineData("12.50", "12.5")]
  [InlineData("12.00", "12")]
  [InlineData("0.0", "0")]
  [InlineData("100", "100")]
  [InlineData("-2.500", "-2.5")]
  public void I16_ANonNormalisedButLegalDecimal_IsAcceptedAndNormalised(
    string received,
    string expected
  )
  {
    var json = ConstDecimalJson(received);

    var node = Assert.IsType<ConstExpr>(
      JsonSerializer.Deserialize<Expr>(json, DslJsonOptions.Options)
    );

    Assert.Equal(expected, node.Value.Decimal);
  }

  [Theory]
  [InlineData("1e3")]
  [InlineData("+5")]
  [InlineData("007")]
  [InlineData(" 5 ")]
  [InlineData("1,5")]
  [InlineData("")]
  [InlineData(".5")]
  [InlineData("5.")]
  // Section 5 refuses negative zero outright, including the forms that only normalise to it.
  [InlineData("-0")]
  [InlineData("-0.0")]
  [InlineData("-0.000")]
  public void ADecimalOutsideTheGrammar_IsRejected(string illegal)
  {
    var json = ConstDecimalJson(illegal);

    Assert.ThrowsAny<JsonException>(() =>
      JsonSerializer.Deserialize<Expr>(json, DslJsonOptions.Options)
    );
  }

  // ---------------------------------------------------------------- I-10

  [Fact]
  public void I10_AnUnknownPropertyOnANode_IsIgnored()
  {
    const string json = """{"kind":"ref","key":"level","comment":"written by a newer writer"}""";

    var node = Assert.IsType<RefExpr>(
      JsonSerializer.Deserialize<Expr>(json, DslJsonOptions.Options)
    );

    Assert.Equal("level", node.Key);
  }

  [Fact]
  public void AConstValueWithZeroOrTwoKeys_IsRejected()
  {
    Assert.ThrowsAny<JsonException>(() =>
      JsonSerializer.Deserialize<Expr>("""{"kind":"const","value":{}}""", DslJsonOptions.Options)
    );
    Assert.ThrowsAny<JsonException>(() =>
      JsonSerializer.Deserialize<Expr>(
        """{"kind":"const","value":{"int":1,"bool":true}}""",
        DslJsonOptions.Options
      )
    );
  }

  // ------------------------------------------------- E-03, literal property names

  [Fact]
  public void PropertyNames_AreLiteral_WhateverTheNamingPolicy()
  {
    var withoutPolicy = new JsonSerializerOptions
    {
      Converters = { new ExprJsonConverter(), new DslConstValueJsonConverter() },
    };

    var tree = SampleTree();
    var json = JsonSerializer.Serialize(tree, withoutPolicy);
    var back = JsonSerializer.Deserialize<Expr>(json, withoutPolicy);

    Assert.NotNull(back);
    Assert.Equal(
      CanonicalJson.ComputeCanonicalSha256(tree),
      CanonicalJson.ComputeCanonicalSha256(back!)
    );
  }
}
