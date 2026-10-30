using System.Numerics;

namespace OpenGta2.Physics.Tests;

/// <summary>
/// Hand-built world for tests: a list of walls and a function describing the surfaces at a point.
/// </summary>
public class FakeWorld : ICollisionWorld
{
    public List<WallSegment> Walls { get; } = new();

    /// <summary>
    /// Surfaces at (x, y). Defaults to a flat floor at height 0.
    /// </summary>
    public Func<float, float, IEnumerable<Surface>> SurfaceFunc { get; set; } = (_, _) => new[] { new Surface(0, float.PositiveInfinity) };

    public FakeWorld Wall(float ax, float ay, float bx, float by, float zMin = 0, float zMax = 8)
    {
        Walls.Add(new WallSegment(new Vector2(ax, ay), new Vector2(bx, by), zMin, zMax));
        return this;
    }

    public void GetWalls(Bounds2 bounds, float zMin, float zMax, List<WallSegment> results)
    {
        foreach (var wall in Walls)
        {
            if (wall.ZMax < zMin || wall.ZMin > zMax) continue;
            results.Add(wall);
        }
    }

    public void GetSurfaces(float x, float y, List<Surface> results)
    {
        results.AddRange(SurfaceFunc(x, y).OrderBy(s => s.Height));
    }
}
