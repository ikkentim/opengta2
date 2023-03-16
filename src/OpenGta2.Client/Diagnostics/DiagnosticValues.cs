using System.Collections.Generic;
using System.Diagnostics;

namespace OpenGta2.Client.Diagnostics;

public static class DiagnosticValues
{
    private static readonly Dictionary<string, string> _values = new();

    public static IReadOnlyDictionary<string, string> Values => _values;

    [Conditional("DEBUG")]
    public static void Set(string key, string value) => _values[key] = value;
}