namespace TableHall.Dsl.Compilation;

public interface ISymbolResolver
{
  bool TryResolve(string key, out Symbol symbol);
}
