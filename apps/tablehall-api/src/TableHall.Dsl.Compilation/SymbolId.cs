using System;

namespace TableHall.Dsl.Compilation;

public readonly struct SymbolId : IEquatable<SymbolId>
{
  public Guid Value { get; }

  public SymbolId(Guid value) => Value = value;

  public static SymbolId New() => new(Guid.NewGuid());

  public bool Equals(SymbolId other) => Value.Equals(other.Value);

  public override bool Equals(object? obj) => obj is SymbolId other && Equals(other);

  public override int GetHashCode() => Value.GetHashCode();

  public override string ToString() => Value.ToString();

  public static implicit operator Guid(SymbolId id) => id.Value;

  public static explicit operator SymbolId(Guid value) => new(value);
}
