using System.Numerics;

namespace OpenGta2.Physics;

/// <summary>
/// A solid vertical wall, as a 2D segment with a vertical extent. Walls are treated as two-sided.
/// </summary>
public readonly record struct WallSegment(Vector2 A, Vector2 B, float ZMin, float ZMax);

/// <summary>
/// A walkable surface at some (x, y).
/// </summary>
/// <param name="Height">Height of the surface.</param>
/// <param name="Headroom">Free distance above the surface until the next solid thing (infinity if open sky).</param>
/// <param name="Standable">
/// Whether it is a sensible place to put a character, for example after a fall rescue. Roofs and water are solid (a character
/// that ends up there doesn't fall through) but not standable.
/// </param>
public readonly record struct Surface(float Height, float Headroom, bool Standable = true);

public readonly record struct Bounds2(Vector2 Min, Vector2 Max)
{
    public Bounds2 Expand(float amount) => new(Min - new Vector2(amount), Max + new Vector2(amount));
}

/// <summary>
/// The static world as seen by the physics code. Implemented on top of the map in the client and by fakes in tests.
/// </summary>
public interface ICollisionWorld
{
    /// <summary>
    /// Adds all walls that intersect <paramref name="bounds"/> and overlap the vertical range
    /// [<paramref name="zMin"/>, <paramref name="zMax"/>] to <paramref name="results"/>. Areas outside the world should be reported as walls.
    /// </summary>
    void GetWalls(Bounds2 bounds, float zMin, float zMax, List<WallSegment> results);

    /// <summary>
    /// Adds all walkable surfaces at (x, y) to <paramref name="results"/>, ordered bottom to top.
    /// </summary>
    void GetSurfaces(float x, float y, List<Surface> results);
}
