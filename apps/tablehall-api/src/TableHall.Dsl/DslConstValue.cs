using System;
using System.Text.Json.Serialization;

namespace TableHall.Dsl;

[JsonConverter(typeof(DslConstValueJsonConverter))]
public sealed record DslConstValue
{
  public int? Int { get; init; }
  public string? Decimal { get; init; }
  public bool? Bool { get; init; }
  public string? String { get; init; }

  public static DslConstValue FromInt(int value) => new() { Int = value };

  public static DslConstValue FromDecimal(decimal value) =>
    new() { Decimal = value.ToString(System.Globalization.CultureInfo.InvariantCulture) };

  public static DslConstValue FromBool(bool value) => new() { Bool = value };

  public static DslConstValue FromString(string value) => new() { String = value };

  public object? GetValue()
  {
    if (Int is not null)
      return Int;
    if (Decimal is not null)
      return Decimal;
    if (Bool is not null)
      return Bool;
    if (String is not null)
      return String;
    return null;
  }
}
