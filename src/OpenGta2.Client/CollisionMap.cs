using System;
using Microsoft.Xna.Framework;
using OpenGta2.GameData.Map;

namespace OpenGta2.Client;

public class CollisionMap
{
    private readonly Map _map;

    public CollisionMap(Map map)
    {
        _map = map;
    }

    public Vector3 CalcMovement(Vector3 from, Vector2 delta)
    {
        var cellFrom = IntVector3.Floor(from);
        var cellTo = IntVector3.Floor(from + new Vector3(delta, 0));

        if (cellFrom == cellTo)
        {
            var cell = _map.GetBlock(cellFrom.X, cellFrom.Y, cellFrom.Z);

            if (cell.Lid.TileGraphic == 0)
            {
                // empty cell - free to move
                return from + new Vector3(delta, 0);
            }
            // TODO support for various slope types
        }
        else
        {
            // TODO: visit cells and check for collision

            var xy = new Vector2(from.X, from.Y);
            GridVisit(xy, xy + delta, x =>
            {

            });
        }
        return from + new Vector3(delta, 0);
    }

    public void GridVisit(Vector2 start, Vector2 end, Action<IntVector2> visit)
    {
        var difX = end.X - start.X;
        var difY = end.Y - start.Y;
        var dist = MathF.Abs(difX) + MathF.Abs(difY);

        var dx = difX / dist;
        var dy = difY / dist;

        var ceilDist = (int)MathF.Ceiling(dist);
        for (var i = 0; i <= ceilDist; i++)
        {
            var x = (int)MathF.Floor(start.X + dx * i);
            var y = (int)MathF.Floor(start.Y + dy * i);
            visit(new IntVector2(x, y));
        }
    }
}