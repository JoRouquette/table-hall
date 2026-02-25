using System.Text.Json;
using TableHall.Dsl;
using Xunit;

namespace TableHall.Dsl.Tests;

public class JsonRoundTripTests
{
  [Fact]
  public void Expr_RoundTrip_And_CanonicalHash_Stable()
  {
    var expr = new BinaryExpr(
      "+",
      new ConstExpr(DslConstValue.FromInt(1)),
      new ConstExpr(DslConstValue.FromDecimal(2.5m))
    );
    var json = JsonSerializer.Serialize(expr, DslJsonOptions.Options);
    var expr2 = JsonSerializer.Deserialize<Expr>(json, DslJsonOptions.Options);
    Assert.NotNull(expr2);
    Assert.Equal(expr.Kind, expr2!.Kind);
    var hash1 = CanonicalJson.ComputeCanonicalSha256(expr);
    var hash2 = CanonicalJson.ComputeCanonicalSha256(expr2);
    Assert.Equal(hash1, hash2);
  }
}
