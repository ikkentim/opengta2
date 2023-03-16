using System;
using Microsoft.Xna.Framework;

namespace OpenGta2.Client;

public readonly struct LineSegment2D
{
    public LineSegment2D(Vector2 from, Vector2 to)
    {
        From = from;
        To = to;
    }

    public Vector2 From { get; }
    public Vector2 To { get; }
    
    public static Vector2 Intersection(LineSegment2D a, LineSegment2D b)
    {
        var c = a.GetComponents();
        var d = b.GetComponents();

        var u = c.Y * d.Z - d.Y * c.Z;
        var v = d.X * c.Z - c.X * d.Z;
        var w = c.X * d.Y - d.X * c.Y;

        return new Vector2(u / w, v / w);
    }

    public float DistanceToLineSquared(Vector2 point)
    {
        var l2 = (From - To).LengthSquared();
        if (l2 == 0.0f) return (point - From).LengthSquared();

        var t = MathF.Max(0, MathF.Min(1, Vector2.Dot(point - From, To - From) / l2));
        var projection = From + t * (To - From);
        return (point - projection).LengthSquared();
    }

    private Vector3 GetComponents()
    {
        var a = From.Y - To.Y;
        var b = To.X - From.X;
        var c = From.X * To.Y - From.Y * To.X;

        return new Vector3(a, b, c);
    }
}