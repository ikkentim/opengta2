using Microsoft.Xna.Framework;
using OpenGta2.Client.Diagnostics;
using OpenGta2.Client.Levels;
using OpenGta2.GameData.Map;

namespace OpenGta2.Client;

public class CollisionMap
{
    private readonly Map _map;

    public CollisionMap(Map map)
    {
        _map = map;
    }
    
    public Vector3 CalculateMovement(Vector3 point, float radius, Vector2 delta)
    {
        var z = (int)point.Z;

        var result = CalculateMovement(new Vector2(point.X, point.Y), z, radius, delta);

        return new Vector3(result, point.Z);
    }

    public Vector2 CalculateMovement(Vector2 point, int z, float radius, Vector2 delta)
    {
        var target = point + delta;

        var min = Vector2.Min(point, target) - new Vector2(radius);
        var max = Vector2.Max(point, target) + new Vector2(radius);
        
        var minInt = IntVector2.Floor(min);
        var maxInt = IntVector2.Ceiling(max);

        var wall = GetCollidingWall(minInt, maxInt, z, point, radius, delta);

        if (wall != null)
        {
            return point;
        }

        return target;
    }

    private Wall? GetCollidingWall(IntVector2 min, IntVector2 max, int z, Vector2 point, float radius, Vector2 delta)
    {
        var target = point + delta;

        var movement = new LineSegment2D(point, target);

        var radSq = radius * radius;

        for (var x = min.X; x <= max.X; x++)
        {
            for (var y = min.Y; y <= max.Y; y++)
            {
                var cell = new IntVector2(x, y);

                //  --c
                // |  |
                // a--b
                var a = cell + Vector2.UnitY;
                var b = cell + Vector2.One;
                var c = cell + Vector2.UnitX;

                // bottom
                var line = new LineSegment2D(a, b);
                var intersection = LineSegment2D.Intersection(movement, line);
                
                var intersectDelta = intersection - point;

                if (Vector2.Dot(delta, intersectDelta) <= 0)
                {
                    // wall is behind player

                    DiagnosticHighlight.Add(new Vector3(intersection, z), GtaVector.Skywards, Color.Red); // intersection

                    continue;
                }
                
                DiagnosticHighlight.Add(new Vector3(intersection, z), GtaVector.Skywards, Color.Yellow); // intersection

                var maxDelta = intersectDelta.Length();
                var deltaLen = delta.Length();
                
                if (intersection.X >= a.X - radius && intersection.X <= b.X + radius && deltaLen > maxDelta)
                {
                    var block1 = _map.TryGetBlock(x, y, z);
                    var block2 = _map.TryGetBlock(x, y + 1, z);

                    if ((block1?.Bottom.Wall ?? false) || (block2?.Top.Wall ?? false))
                    {
                        // intersect!
                        DiagnosticHighlight.Add(new IntVector3(cell.X, cell.Y, z), Color.Red); // hit block
                        return new Wall(cell, false);
                    }
                }
            }
        }

        return null;
    }
    
    /// <param name="Cell"></param>
    /// <param name="Right">if false, bottom</param>
    private record struct Wall(IntVector2 Cell, bool Right);
}