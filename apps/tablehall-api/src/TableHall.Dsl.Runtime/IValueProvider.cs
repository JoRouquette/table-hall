using System.Collections.Generic;
using TableHall.Dsl;
using TableHall.Dsl.Compilation;

namespace TableHall.Dsl.Runtime;

public interface IValueProvider
{
  bool TryGetScalar(SymbolId id, out DslValue value);
  bool TryGetCollection(SymbolId id, out IReadOnlyList<DslValue> values);
}
