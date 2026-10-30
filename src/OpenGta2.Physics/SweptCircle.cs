using System.Numerics;

namespace OpenGta2.Physics;

public readonly record struct SweepHit(float Time, Vector2 Normal);

/// <summary>
/// Continuous collision tests of a moving circle against static line segments.
/// </summary>
public static class SweptCircle
{
    /// <summary>
    /// Tolerance used to treat a circle as "touching" a segment instead of penetrating it.
    /// </summary>
    public const float Epsilon = 1e-4f;

    /// <summary>
    /// Finds the first time in [0, 1] at which a circle of <paramref name="radius"/> at <paramref name="start"/>,
    /// moving by <paramref name="delta"/>, touches the segment a-b while moving towards it.
    /// The returned normal points from the segment to the circle.
    /// </summary>
    public static SweepHit? Sweep(Vector2 start, Vector2 delta, float radius, Vector2 a, Vector2 b)
    {
        SweepHit? best = null;

        // the flat sides of the capsule
        var ab = b - a;
        var lengthSquared = ab.LengthSquared();
        if (lengthSquared > 0)
        {
            var length = MathF.Sqrt(lengthSquared);
            var tangent = ab / length;
            var normal = new Vector2(-tangent.Y, tangent.X);

            var distance = Vector2.Dot(start - a, normal);
            var speed = Vector2.Dot(delta, normal);
            var side = distance >= 0 ? 1f : -1f;

            // moving towards the line, and not already on the other side of it
            if (side * speed < 0 && side * distance >= radius - Epsilon)
            {
                var t = MathF.Max(0, (side * radius - distance) / speed);
                if (t <= 1)
                {
                    var along = Vector2.Dot(start + delta * t - a, tangent);
                    if (along >= 0 && along <= length)
                    {
                        best = new SweepHit(t, normal * side);
                    }
                }
            }
        }

        // the rounded ends
        Consider(ref best, SweepPoint(start, delta, radius, a));
        Consider(ref best, SweepPoint(start, delta, radius, b));

        return best;
    }

    /// <summary>
    /// Sweeps a circle against a single point.
    /// </summary>
    public static SweepHit? SweepPoint(Vector2 start, Vector2 delta, float radius, Vector2 point)
    {
        var f = start - point;
        var a = delta.LengthSquared();
        if (a == 0) return null;

        var b = Vector2.Dot(f, delta);
        var c = f.LengthSquared() - radius * radius;

        if (b >= 0)
        {
            // moving away from (or perpendicular to) the point
            return null;
        }

        if (c <= 0)
        {
            // already touching (or slightly penetrating) and moving closer
            if (c < -2 * radius * Epsilon - Epsilon)
            {
                return null; // deeply penetrating; depenetration is responsible for this
            }

            return new SweepHit(0, Vector2.Normalize(f));
        }

        var discriminant = b * b - a * c;
        if (discriminant < 0) return null;

        var t = (-b - MathF.Sqrt(discriminant)) / a;
        if (t < 0 || t > 1) return null;

        var contact = f + delta * t;
        return new SweepHit(t, contact / radius);
    }

    private static void Consider(ref SweepHit? best, SweepHit? candidate)
    {
        if (candidate.HasValue && (!best.HasValue || candidate.Value.Time < best.Value.Time))
        {
            best = candidate;
        }
    }

    /// <summary>
    /// Returns the closest point on segment a-b to <paramref name="point"/>.
    /// </summary>
    public static Vector2 ClosestPointOnSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        var lengthSquared = ab.LengthSquared();
        if (lengthSquared == 0) return a;

        var t = Math.Clamp(Vector2.Dot(point - a, ab) / lengthSquared, 0, 1);
        return a + ab * t;
    }
}
