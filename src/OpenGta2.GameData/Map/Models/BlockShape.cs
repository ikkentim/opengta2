using System.Numerics;

namespace OpenGta2.GameData.Map;

/// <summary>
/// Collision geometry of blocks that are not plain full cubes, mirroring what the renderer generates.
/// Coordinates are relative to the block: x and y from 0 to 1, y grows downwards.
/// </summary>
public static class BlockShape
{
    /// <summary>
    /// Lid tile number the map uses to mark the 3-sided diagonal slope (a wedge without a lid).
    /// </summary>
    public const ushort WedgeLidTile = 1023;

    private const float PartialWidth = 24 / 64f;
    private const float CentreWidth = 16 / 64f;

    private static readonly Vector2[] _none = Array.Empty<Vector2>();

    private static readonly Vector2[] _left = Rect(0, 0, PartialWidth, 1);
    private static readonly Vector2[] _top = Rect(0, 0, 1, PartialWidth);
    private static readonly Vector2[] _right = Rect(1 - PartialWidth, 0, 1, 1);
    private static readonly Vector2[] _bottom = Rect(0, 1 - PartialWidth, 1, 1);
    private static readonly Vector2[] _topLeft = Rect(0, 0, PartialWidth, PartialWidth);
    private static readonly Vector2[] _topRight = Rect(1 - PartialWidth, 0, 1, PartialWidth);
    private static readonly Vector2[] _bottomRight = Rect(1 - PartialWidth, 1 - PartialWidth, 1, 1);
    private static readonly Vector2[] _bottomLeft = Rect(0, 1 - PartialWidth, PartialWidth, 1);
    private static readonly Vector2[] _centre = Rect((1 - CentreWidth) / 2, (1 - CentreWidth) / 2, (1 + CentreWidth) / 2, (1 + CentreWidth) / 2);

    // the solid half of the block lies on the side opposite to the direction the diagonal face is facing
    private static readonly Vector2[] _diagonalUpRight = { new(0, 0), new(1, 1), new(0, 1) };
    private static readonly Vector2[] _diagonalDownRight = { new(1, 0), new(0, 1), new(0, 0) };
    private static readonly Vector2[] _diagonalDownLeft = { new(1, 1), new(0, 0), new(1, 0) };
    private static readonly Vector2[] _diagonalUpLeft = { new(0, 1), new(1, 0), new(1, 1) };

    /// <summary>
    /// Returns the convex polygon covered by the solid part of a block that does not fill the whole cell
    /// (partial blocks and diagonal blocks). The solid part spans the full height of the block.
    /// Returns an empty array for every other block type.
    /// </summary>
    public static ReadOnlySpan<Vector2> GetSolidPolygon(SlopeType slope)
    {
        return slope switch
        {
            SlopeType.PartialBlockLeft => _left,
            SlopeType.PartialBlockTop => _top,
            SlopeType.PartialBlockRight => _right,
            SlopeType.PartialBlockBottom => _bottom,
            SlopeType.PartialBlockTopLeftCorner => _topLeft,
            SlopeType.PartialBlockTopRightCorner => _topRight,
            SlopeType.PartialBlockBottomRightCorner => _bottomRight,
            SlopeType.PartialBlockBottomLeftCorner => _bottomLeft,
            SlopeType.PartialBlockCentre => _centre,
            SlopeType.DiagonalFacingUpRight => _diagonalUpRight,
            SlopeType.DiagonalFacingDownRight => _diagonalDownRight,
            SlopeType.DiagonalFacingDownLeft => _diagonalDownLeft,
            SlopeType.DiagonalFacingUpLeft => _diagonalUpLeft,
            _ => _none
        };
    }

    /// <summary>
    /// Whether a point lies inside a convex polygon.
    /// </summary>
    public static bool Contains(ReadOnlySpan<Vector2> polygon, float x, float y)
    {
        var sign = 0f;
        for (var i = 0; i < polygon.Length; i++)
        {
            var a = polygon[i];
            var b = polygon[(i + 1) % polygon.Length];
            var cross = (b.X - a.X) * (y - a.Y) - (b.Y - a.Y) * (x - a.X);

            if (cross == 0) continue;
            if (sign == 0) sign = MathF.Sign(cross);
            else if (MathF.Sign(cross) != sign) return false;
        }

        return true;
    }

    /// <summary>
    /// Returns the height (relative to the bottom of the block) of the walkable top surface at the specified position
    /// within the block, or null if the block has no surface there (for example next to a partial block).
    /// </summary>
    /// <param name="slope">The slope type of the block.</param>
    /// <param name="lidTile">The lid tile of the block. Distinguishes the two kinds of diagonal slope.</param>
    public static float? GetSurfaceHeight(SlopeType slope, ushort lidTile, float x, float y)
    {
        var polygon = GetSolidPolygon(slope);
        if (polygon.Length > 0)
        {
            return Contains(polygon, x, y) ? 1 : null;
        }

        switch (slope)
        {
            case SlopeType.DiagonalSlopeFacingUpRight:
                return DiagonalSlope(lidTile, x, y);
            case SlopeType.DiagonalSlopeFacingDownRight:
                return DiagonalSlope(lidTile, y, 1 - x);
            case SlopeType.DiagonalSlopeFacingDownLeft:
                return DiagonalSlope(lidTile, 1 - x, 1 - y);
            case SlopeType.DiagonalSlopeFacingUpLeft:
                return DiagonalSlope(lidTile, 1 - y, x);
            default:
                return SlopeHeight.Get(slope, x, y);
        }
    }

    /// <summary>
    /// Height of a diagonal slope in its base orientation (facing up-right; the other orientations are rotations of this one).
    /// </summary>
    private static float? DiagonalSlope(ushort lidTile, float x, float y)
    {
        if (lidTile == WedgeLidTile)
        {
            // 3-sided: a wedge that rises from the diagonal line towards the bottom-left corner. Open on the other side.
            return x <= y ? y - x : null;
        }

        // 4-sided: flat on the bottom-left half, sloping down from the diagonal towards the top-right corner
        return 1 - MathF.Max(0, x - y);
    }

    private static Vector2[] Rect(float minX, float minY, float maxX, float maxY) =>
        new Vector2[] { new(minX, minY), new(maxX, minY), new(maxX, maxY), new(minX, maxY) };
}
