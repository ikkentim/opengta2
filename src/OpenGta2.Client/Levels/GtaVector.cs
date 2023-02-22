using Microsoft.Xna.Framework;

namespace OpenGta2.Client.Levels;

public static class GtaVector
{
    private static readonly Vector3 _left = -Vector3.UnitX;
    private static readonly Vector3 _right = Vector3.UnitX;
    private static readonly Vector3 _up = -Vector3.UnitY;
    private static readonly Vector3 _down = Vector3.UnitY;
    private static readonly Vector3 _sky = Vector3.UnitZ;

    /// <summary>
    /// (-1, 0, 0)
    /// </summary>
    public static Vector3 Left => _left;

    /// <summary>
    /// (1, 0, 0)
    /// </summary>
    public static Vector3 Right => _right;

    /// <summary>
    /// (0, -1, 0)
    /// </summary>
    public static Vector3 Up => _up;

    /// <summary>
    /// (0, 1, 0)
    /// </summary>
    public static Vector3 Down => _down;

    /// <summary>
    /// (0, 0, 1)
    /// </summary>
    public static Vector3 Skywards => _sky;
}