namespace OpenGta2.GameData.Map;

/// <summary>
/// An animation of a tile, such as moving water or a flickering neon sign. The map uses <see cref="Base"/> as the tile number
/// of the animated tile; the animation shows the tiles in <see cref="Tiles"/> in turn in its place.
/// </summary>
public struct TileAnimation
{
    public ushort Base;

    /// <summary>
    /// The number of game frames each frame of the animation is shown for.
    /// </summary>
    public byte FrameRate;

    /// <summary>
    /// The number of times the animation is played; 0 means forever.
    /// </summary>
    public byte Repeat;

    public ushort[] Tiles;

    /// <summary>
    /// Returns the index in <see cref="Tiles"/> of the frame that is shown at the specified game frame (counted from the start
    /// of the animation). Animations that play a limited number of times stay on their last frame when they are done.
    /// </summary>
    public readonly int GetFrameIndex(long gameFrame)
    {
        if (Tiles == null || Tiles.Length == 0 || gameFrame < 0)
        {
            return 0;
        }

        var framesPerStep = Math.Max(1, (int)FrameRate);
        var step = gameFrame / framesPerStep;

        if (Repeat > 0 && step >= (long)Repeat * Tiles.Length)
        {
            return Tiles.Length - 1;
        }

        return (int)(step % Tiles.Length);
    }

    /// <summary>
    /// Returns the number of the tile that is shown at the specified game frame.
    /// </summary>
    public readonly ushort GetTile(long gameFrame) => Tiles == null || Tiles.Length == 0 ? Base : Tiles[GetFrameIndex(gameFrame)];
}
