using System;
using System.Globalization;
using System.Text.Json;

namespace TableHall.Dsl;

/// <summary>
/// ABI version carried by every durable payload (contract section 1). A reader that meets a major
/// version above its own refuses to read: it does not try to interpret what it does not know.
/// </summary>
public static class DslAbi
{
  public const string Version = "1.0.0";

  public static int MajorOf(string abiVersion)
  {
    if (string.IsNullOrWhiteSpace(abiVersion))
      throw new JsonException("abiVersion is missing");

    var firstDot = abiVersion.IndexOf('.');
    var majorText = firstDot < 0 ? abiVersion : abiVersion[..firstDot];

    if (!int.TryParse(majorText, NumberStyles.None, CultureInfo.InvariantCulture, out var major))
      throw new JsonException($"'{abiVersion}' is not a readable ABI version");

    return major;
  }

  public static void EnsureReadable(string abiVersion)
  {
    var major = MajorOf(abiVersion);
    var supported = MajorOf(Version);

    if (major > supported)
      throw new JsonException(
        $"payload declares ABI {abiVersion}, this build only reads major version {supported}"
      );
  }
}
