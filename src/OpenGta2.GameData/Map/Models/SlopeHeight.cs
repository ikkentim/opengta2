namespace OpenGta2.GameData.Map;

/// <summary>
/// Computes the height of the top surface of a block with a slope, mirroring the geometry the renderer generates.
/// </summary>
public static class SlopeHeight
{
    /// <summary>
    /// Returns the height (0 to 1, relative to the bottom of the block) of the walkable top surface of a block with the
    /// specified <paramref name="slope"/> at the specified position within the block (<paramref name="x"/> and <paramref name="y"/> from 0 to 1;
    /// y grows downwards).
    /// </summary>
    /// <remarks>
    /// Only the straight slopes (45, 26 and 7 degrees) have a varying height. All other block types return 1.
    /// Diagonal and partial blocks are not handled yet.
    /// </remarks>
    public static float Get(SlopeType slope, float x, float y)
    {
        if (!TryGetSlope(slope, out var direction, out var from, out var to))
        {
            return 1;
        }

        // progress from the low edge (0) to the high edge (1)
        var progress = direction switch
        {
            Direction.Up => 1 - y,
            Direction.Down => y,
            Direction.Left => 1 - x,
            _ => x
        };

        return from + (to - from) * Math.Clamp(progress, 0, 1);
    }

    private enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    private static bool TryGetSlope(SlopeType slope, out Direction direction, out float from, out float to)
    {
        direction = Direction.Up;
        from = 0;
        to = 1;

        switch (slope)
        {
            case SlopeType.Up45: return true;
            case SlopeType.Down45: direction = Direction.Down; return true;
            case SlopeType.Left45: direction = Direction.Left; return true;
            case SlopeType.Right45: direction = Direction.Right; return true;

            case SlopeType.Up26_1: case SlopeType.Up26_2:
                return Section(slope - SlopeType.Up26_1, 2, Direction.Up, out direction, out from, out to);
            case SlopeType.Down26_1: case SlopeType.Down26_2:
                return Section(slope - SlopeType.Down26_1, 2, Direction.Down, out direction, out from, out to);
            case SlopeType.Left26_1: case SlopeType.Left26_2:
                return Section(slope - SlopeType.Left26_1, 2, Direction.Left, out direction, out from, out to);
            case SlopeType.Right26_1: case SlopeType.Right26_2:
                return Section(slope - SlopeType.Right26_1, 2, Direction.Right, out direction, out from, out to);

            case >= SlopeType.Up7_1 and <= SlopeType.Up7_8:
                return Section(slope - SlopeType.Up7_1, 8, Direction.Up, out direction, out from, out to);
            case >= SlopeType.Down7_1 and <= SlopeType.Down7_8:
                return Section(slope - SlopeType.Down7_1, 8, Direction.Down, out direction, out from, out to);
            case >= SlopeType.Left7_1 and <= SlopeType.Left7_8:
                return Section(slope - SlopeType.Left7_1, 8, Direction.Left, out direction, out from, out to);
            case >= SlopeType.Right7_1 and <= SlopeType.Right7_8:
                return Section(slope - SlopeType.Right7_1, 8, Direction.Right, out direction, out from, out to);

            default:
                return false;
        }
    }

    /// <summary>
    /// A gentle slope spans several blocks; each block covers section <paramref name="index"/> of <paramref name="count"/>.
    /// </summary>
    private static bool Section(int index, int count, Direction direction, out Direction outDirection, out float from, out float to)
    {
        outDirection = direction;
        from = index / (float)count;
        to = (index + 1) / (float)count;
        return true;
    }
}
