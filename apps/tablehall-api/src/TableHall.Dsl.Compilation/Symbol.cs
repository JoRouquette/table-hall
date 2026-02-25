using TableHall.Dsl;

namespace TableHall.Dsl.Compilation;

public sealed record Symbol(SymbolId Id, string Key, DslType Type, SymbolKind Kind);
