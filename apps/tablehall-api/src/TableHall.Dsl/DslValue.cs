using System;

namespace TableHall.Dsl;

public abstract record DslValue
{
  public sealed record Int(int Value) : DslValue;

  public sealed record Decimal(decimal Value) : DslValue;

  public sealed record Bool(bool Value) : DslValue;

  public sealed record String(string Value) : DslValue;

  public int? TryAsInt() => this is Int i ? i.Value : null;

  public decimal? TryAsDecimal() => this is Decimal d ? d.Value : null;

  public bool? TryAsBool() => this is Bool b ? b.Value : null;

  public string? TryAsString() => this is String s ? s.Value : null;
}
