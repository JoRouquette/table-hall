using System.Collections.Generic;
using TableHall.Dsl;
using TableHall.Dsl.Compilation;

namespace TableHall.Dsl.Tests;

public static class Dnd5eProfile
{
  public static DslProfileDefinition Minimal =>
    new()
    {
      AllowedBinaryOps = new[] { "+", "-", "*", "/", "==", "!=", "<=", ">=", "<", ">" },
      AllowedUnaryOps = new[] { "-", "not" },
      BuiltinFunctions = new[] { "toInt", "toDecimal", "round" },
      RoundingModes = new[]
      {
        "Floor",
        "Ceiling",
        "TowardZero",
        "AwayFromZero",
        "HalfUp",
        "HalfEven",
      },
      UserFunctions = new List<DslProfileDefinition.UserFunction>
      {
        new(
          "abilityMod",
          new[] { "score" },
          new DslType(DslPrimitiveType.Int),
          new CallExpr(
            "toInt",
            new Expr[]
            {
              new BinaryExpr(
                "/",
                new BinaryExpr("-", new RefExpr("score"), new ConstExpr(DslConstValue.FromInt(10))),
                new ConstExpr(DslConstValue.FromInt(2))
              ),
              new ConstExpr(DslConstValue.FromString("Floor")),
            }
          )
        ),
        new(
          "proficiencyBonus",
          new[] { "level" },
          new DslType(DslPrimitiveType.Int),
          new IfExpr(
            new BinaryExpr("<=", new RefExpr("level"), new ConstExpr(DslConstValue.FromInt(4))),
            new ConstExpr(DslConstValue.FromInt(2)),
            new IfExpr(
              new BinaryExpr("<=", new RefExpr("level"), new ConstExpr(DslConstValue.FromInt(8))),
              new ConstExpr(DslConstValue.FromInt(3)),
              new IfExpr(
                new BinaryExpr(
                  "<=",
                  new RefExpr("level"),
                  new ConstExpr(DslConstValue.FromInt(12))
                ),
                new ConstExpr(DslConstValue.FromInt(4)),
                new IfExpr(
                  new BinaryExpr(
                    "<=",
                    new RefExpr("level"),
                    new ConstExpr(DslConstValue.FromInt(16))
                  ),
                  new ConstExpr(DslConstValue.FromInt(5)),
                  new ConstExpr(DslConstValue.FromInt(6))
                )
              )
            )
          )
        ),
        new(
          "attackBonus",
          new[] { "abilityScore", "level", "proficient" },
          new DslType(DslPrimitiveType.Int),
          new BinaryExpr(
            "+",
            new CallExpr("abilityMod", new Expr[] { new RefExpr("abilityScore") }),
            new IfExpr(
              new RefExpr("proficient"),
              new CallExpr("proficiencyBonus", new Expr[] { new RefExpr("level") }),
              new ConstExpr(DslConstValue.FromInt(0))
            )
          )
        ),
      },
    };
}
