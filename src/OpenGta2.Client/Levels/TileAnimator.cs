using System;
using OpenGta2.GameData.Map;

namespace OpenGta2.Client.Levels;

/// <summary>
/// Plays the tile animations of a map (moving water, flickering neon signs, ...) by changing the graphics of the animated tiles.
/// </summary>
public class TileAnimator
{
    /// <summary>
    /// The number of game frames per second. The map format counts animation speed in game frames, but does not say how long
    /// they are.
    /// </summary>
    /// <remarks>
    /// Measured from a recording of the original game on level 3 (bil), standing still next to water: the 12-frame water animation
    /// (3 game frames per frame on that level) advanced every 112.8 ms (197 steps in 22.1 s), which is 26.6 game frames per second.
    /// That is a little below the 30 that fan reports give as the target of the game (a speedrun forum measures 30 to 30.3, a GTAMP forum
    /// post says 30, or 50 in the "fast" mode of some versions), so the original may run slower than its target on a given machine.
    /// </remarks>
    public const double DefaultGameFramesPerSecond = 26.6;

    private readonly TileAnimation[] _animations;
    private readonly StyleTextureSet _textures;
    private readonly int[] _shownFrame;
    private double _gameFrame;

    /// <summary>
    /// The number of game frames per second that animations run at. Can be changed while the game runs to tune the speed
    /// (F8/F9 in the game).
    /// </summary>
    public double GameFramesPerSecond { get; set; } = DefaultGameFramesPerSecond;

    public TileAnimator(TileAnimation[] animations, StyleTextureSet textures)
    {
        _animations = animations;
        _textures = textures;

        // nothing has been uploaded yet; the first update shows the right frame even if it is not the base tile itself
        _shownFrame = new int[animations.Length];
        Array.Fill(_shownFrame, -1);
    }

    public int AnimationCount => _animations.Length;

    /// <summary>
    /// Advances the animations by the specified time and updates the animated tiles.
    /// </summary>
    public void Update(double deltaSeconds)
    {
        // counting game frames (instead of deriving them from the total time) keeps the animations smooth when the speed is changed
        _gameFrame += deltaSeconds * GameFramesPerSecond;
        var gameFrame = (long)_gameFrame;

        for (var i = 0; i < _animations.Length; i++)
        {
            ref readonly var animation = ref _animations[i];
            var index = animation.GetFrameIndex(gameFrame);

            if (index == _shownFrame[i])
            {
                continue;
            }

            _shownFrame[i] = index;
            _textures.CopyTile(animation.Base, animation.Tiles[index]);
        }
    }
}
