using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace OpenGta2.Client.Diagnostics;

public static class DiagnosticHighlight
{
    private static readonly List<(Vector3 block, Vector3 scale, Color color)> _highlightedBlocks = new();

    public static IReadOnlyList<(Vector3 block, Vector3 scale, Color color)> HighlightedBlocks => _highlightedBlocks;
    
    [Conditional("DEBUG")]
    public static void Add(Vector3 point, Vector3 scale, Color color)
    {
        _highlightedBlocks.Add((point, scale, color));
    }
    
    [Conditional("DEBUG")]
    public static void Add(IntVector3 point, Color color)
    {
        Add(point, Vector3.One, color);
    }
    
    public static void Reset()
    {
        _highlightedBlocks.Clear();
    }
}