using System;
using System.Collections.Generic;
using System.Numerics;
using OpenGta2.GameData.Map;
using OpenGta2.Physics;

namespace OpenGta2.Client.Levels;

/// <summary>
/// Exposes a <see cref="Map"/> to the physics code.
/// </summary>
/// <remarks>
/// Walls come from the <c>Wall</c> flag of block side faces, plus the outline of partial and diagonal blocks (see <see cref="BlockShape"/>).
/// Surfaces are the tops of blocks with a lid tile, following slopes and diagonal slopes.
/// </remarks>
public class MapCollisionWorld : ICollisionWorld
{
    private readonly Map _map;

    public MapCollisionWorld(Map map)
    {
        _map = map;
    }

    public void GetWalls(Bounds2 bounds, float zMin, float zMax, List<WallSegment> results)
    {
        var minX = (int)MathF.Floor(bounds.Min.X);
        var maxX = (int)MathF.Floor(bounds.Max.X) + 1; // +1: also the edge at the far side of the last cell
        var minY = (int)MathF.Floor(bounds.Min.Y);
        var maxY = (int)MathF.Floor(bounds.Max.Y) + 1;

        var minLayer = Math.Max(0, (int)MathF.Floor(zMin));
        var maxLayer = Math.Min(7, (int)MathF.Floor(zMax));

        // the edge of the map is solid
        AddMapBorder(minX, maxX, minY, maxY, results);

        for (var x = Math.Max(minX, 0); x <= Math.Min(maxX, _map.Width); x++)
        {
            for (var y = Math.Max(minY, 0); y <= Math.Min(maxY, _map.Height); y++)
            {
                for (var z = minLayer; z <= maxLayer; z++)
                {
                    // each cell is responsible for its Left and Top edge; the far edges belong to the next cell
                    if (HasWall(x, y, z, Face.Left, x - 1, y, Face.Right))
                    {
                        results.Add(new WallSegment(new Vector2(x, y), new Vector2(x, y + 1), z, z + 1));
                    }

                    if (HasWall(x, y, z, Face.Top, x, y - 1, Face.Bottom))
                    {
                        results.Add(new WallSegment(new Vector2(x, y), new Vector2(x + 1, y), z, z + 1));
                    }

                    AddSolidPolygonWalls(x, y, z, results);
                }
            }
        }
    }

    public void GetSurfaces(float x, float y, List<Surface> results)
    {
        var cellX = (int)MathF.Floor(x);
        var cellY = (int)MathF.Floor(y);
        if (!IsInMap(cellX, cellY)) return;

        var column = _map.GetColumn(cellX, cellY);

        var fractionX = x - cellX;
        var fractionY = y - cellY;

        // find the layers that have a lid; those are the walkable tops and the solid undersides
        var previousHeight = float.NaN;
        var previousStandable = true;
        for (var z = column.Offset; z < column.Height; z++)
        {
            ref var block = ref _map.CompressedMap.Blocks[column.Blocks[z - column.Offset]];
            if (block.Lid.TileGraphic == 0) continue;

            if (!float.IsNaN(previousHeight))
            {
                // headroom of the previous surface is the gap up to the (flat) underside of this block
                results[^1] = new Surface(previousHeight, z - previousHeight, previousStandable);
            }

            // no surface here, for example next to a partial block: the block doesn't cover this part of the cell
            var blockHeight = BlockShape.GetSurfaceHeight(block.SlopeType.SlopeType, block.Lid.TileGraphic, fractionX, fractionY);
            if (!blockHeight.HasValue) continue;

            var height = z + blockHeight.Value;
            // blocks on the bottom layer are water, and blocks with ground type air are mostly roofs
            previousStandable = z > 0 && block.SlopeType.GroundType != GroundType.Air;
            results.Add(new Surface(height, float.PositiveInfinity, previousStandable));
            previousHeight = height;
        }
    }

    private void AddMapBorder(int minX, int maxX, int minY, int maxY, List<WallSegment> results)
    {
        const float top = 8;
        if (minX <= 0)
            results.Add(new WallSegment(new Vector2(0, 0), new Vector2(0, _map.Height), 0, top));
        if (maxX >= _map.Width)
            results.Add(new WallSegment(new Vector2(_map.Width, 0), new Vector2(_map.Width, _map.Height), 0, top));
        if (minY <= 0)
            results.Add(new WallSegment(new Vector2(0, 0), new Vector2(_map.Width, 0), 0, top));
        if (maxY >= _map.Height)
            results.Add(new WallSegment(new Vector2(0, _map.Height), new Vector2(_map.Width, _map.Height), 0, top));
    }

    private bool HasWall(int x, int y, int z, Face face, int neighbourX, int neighbourY, Face neighbourFace)
    {
        // edges on the map border are handled separately
        if (x <= 0 && face == Face.Left) return false;
        if (y <= 0 && face == Face.Top) return false;
        if (x >= _map.Width && face == Face.Left) return false;
        if (y >= _map.Height && face == Face.Top) return false;

        // either side of an edge can carry the wall flag
        return HasWallFlag(x, y, z, face) || HasWallFlag(neighbourX, neighbourY, z, neighbourFace);
    }

    private bool HasWallFlag(int x, int y, int z, Face face)
    {
        if (!IsInMap(x, y)) return false;

        // blocks that don't fill the cell get their walls from their shape, not from the faces of the cell
        if (_map.TryGetBlock(x, y, z) is not { } block || BlockShape.GetSolidPolygon(block.SlopeType.SlopeType).Length > 0)
        {
            return false;
        }

        return _map.IsWall(new IntVector3(x, y, z), face);
    }

    /// <summary>
    /// Adds the walls around the solid part of partial blocks and diagonal blocks.
    /// </summary>
    private void AddSolidPolygonWalls(int x, int y, int z, List<WallSegment> results)
    {
        if (!IsInMap(x, y) || _map.TryGetBlock(x, y, z) is not { } block) return;

        var polygon = BlockShape.GetSolidPolygon(block.SlopeType.SlopeType);
        for (var i = 0; i < polygon.Length; i++)
        {
            var a = polygon[i] + new Vector2(x, y);
            var b = polygon[(i + 1) % polygon.Length] + new Vector2(x, y);
            results.Add(new WallSegment(a, b, z, z + 1));
        }
    }

    private bool IsInMap(int x, int y) => x >= 0 && y >= 0 && x < _map.Width && y < _map.Height;

}
