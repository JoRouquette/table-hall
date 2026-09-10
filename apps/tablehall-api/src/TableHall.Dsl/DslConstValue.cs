using System;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace TableHall.Dsl;

[JsonConverter(typeof(DslConstValueJsonConverter))]
public sealed partial record DslConstValue
{
  [JsonPropertyName("int")]
  public int? Int { get; init; }

  [JsonPropertyName("decimal")]
  public string? Decimal { get; init; }

  [JsonPropertyName("bool")]
  public bool? Bool { get; init; }

  [JsonPropertyName("string")]
  public string? String { get; init; }

  /// <summary>
  /// ABI 1.0.0 section 5: an optional minus sign, an integer part without leading zeros, and an
  /// optional fractional part of at least one digit. No exponent, no sign, no separator.
  /// </summary>
  [GeneratedRegex(@"^-?(0|[1-9][0-9]*)(\.[0-9]+)?$", RegexOptions.CultureInvariant)]
  private static partial Regex DecimalLiteralPattern();

  public static bool IsLegalDecimalLiteral(string? literal) =>
    literal is not null && DecimalLiteralPattern().IsMatch(literal);

  /// <summary>
  /// ABI 1.0.0 section 5: drop trailing zeros of the fractional part, then the dot if nothing
  /// remains after it. Textual on purpose — the contract states a textual rule, so the code must
  /// read like the contract rather than lean on decimal arithmetic.
  /// Assumes the literal already passed <see cref="IsLegalDecimalLiteral"/>.
  /// </summary>
  public static string NormaliseDecimalLiteral(string literal)
  {
    var normalised = literal;
    if (normalised.IndexOf('.') >= 0)
    {
      normalised = normalised.TrimEnd('0');
      if (normalised[^1] == '.')
      {
        normalised = normalised[..^1];
      }
    }
    return normalised;
  }

  public static DslConstValue FromInt(int value) => new() { Int = value };

  /// <summary>
  /// Negative zero cannot reach the wire (the contract rejects it), but a caller may legitimately
  /// hand us <c>-0.0m</c>; folding it to "0" here keeps this factory unable to produce an illegal
  /// literal.
  /// </summary>
  public static DslConstValue FromDecimal(decimal value)
  {
    var normalised = NormaliseDecimalLiteral(value.ToString(CultureInfo.InvariantCulture));
    if (normalised == "-0")
    {
      normalised = "0";
    }
    return new() { Decimal = normalised };
  }

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
