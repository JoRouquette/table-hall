using System;
using System.Buffers;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TableHall.Dsl;

public static class CanonicalJson
{
  public static string SerializeCanonical(Expr expr)
  {
    var buffer = new ArrayBufferWriter<byte>();
    using var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = false });
    WriteCanonical(expr, writer);
    writer.Flush();
    return Encoding.UTF8.GetString(buffer.WrittenSpan);
  }

  private static void WriteCanonical(object? obj, Utf8JsonWriter writer)
  {
    switch (obj)
    {
      case null:
        writer.WriteNullValue();
        break;
      case Expr e:
        WriteCanonicalExpr(e, writer);
        break;
      case DslConstValue c:
        WriteCanonicalConst(c, writer);
        break;
      case Expr[] arr:
        writer.WriteStartArray();
        foreach (var item in arr)
          WriteCanonical(item, writer);
        writer.WriteEndArray();
        break;
      case IReadOnlyList<Expr> list:
        writer.WriteStartArray();
        foreach (var item in list)
          WriteCanonical(item, writer);
        writer.WriteEndArray();
        break;
      case string s:
        writer.WriteStringValue(s);
        break;
      case int i:
        writer.WriteNumberValue(i);
        break;
      case decimal d:
        writer.WriteStringValue(d.ToString(System.Globalization.CultureInfo.InvariantCulture));
        break;
      case bool b:
        writer.WriteBooleanValue(b);
        break;
      default:
        throw new InvalidOperationException(
          $"Unsupported canonical serialization type: {obj.GetType()}"
        );
    }
  }

  private static void WriteCanonicalExpr(Expr expr, Utf8JsonWriter writer)
  {
    writer.WriteStartObject();
    var props = new SortedDictionary<string, object?>();
    switch (expr)
    {
      case ConstExpr c:
        props["kind"] = c.Kind;
        props["value"] = c.Value;
        break;
      case RefExpr r:
        props["kind"] = r.Kind;
        props["key"] = r.Key;
        break;
      case UnaryExpr u:
        props["kind"] = u.Kind;
        props["op"] = u.Op;
        props["operand"] = u.Operand;
        break;
      case BinaryExpr b:
        props["kind"] = b.Kind;
        props["op"] = b.Op;
        props["left"] = b.Left;
        props["right"] = b.Right;
        break;
      case IfExpr i:
        props["kind"] = i.Kind;
        props["cond"] = i.Cond;
        props["then"] = i.Then;
        props["else"] = i.Else;
        break;
      case CallExpr call:
        props["kind"] = call.Kind;
        props["fn"] = call.Fn;
        props["args"] = call.Args;
        break;
      case AggExpr agg:
        props["kind"] = agg.Kind;
        props["op"] = agg.Op;
        props["source"] = agg.Source;
        break;
      default:
        throw new InvalidOperationException($"Unknown Expr type: {expr.GetType().Name}");
    }
    foreach (var kv in props)
    {
      writer.WritePropertyName(kv.Key);
      WriteCanonical(kv.Value, writer);
    }
    writer.WriteEndObject();
  }

  private static void WriteCanonicalConst(DslConstValue c, Utf8JsonWriter writer)
  {
    writer.WriteStartObject();
    if (c.Int is not null)
      writer.WriteNumber("int", c.Int.Value);
    else if (c.Decimal is not null)
      writer.WriteString("decimal", c.Decimal);
    else if (c.Bool is not null)
      writer.WriteBoolean("bool", c.Bool.Value);
    else if (c.String is not null)
      writer.WriteString("string", c.String);
    else
      throw new InvalidOperationException("DslConstValue must have a value");
    writer.WriteEndObject();
  }

  public static string ComputeCanonicalSha256(Expr expr)
  {
    var canonical = SerializeCanonical(expr);
    using var sha = SHA256.Create();
    var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(canonical));
    return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
  }
}
