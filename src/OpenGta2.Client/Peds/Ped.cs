using Microsoft.Xna.Framework;
using OpenGta2.Physics;

namespace OpenGta2.Client.Peds;

public class Ped
{
    private PedAnimation _animation;

    /// <summary>
    /// Movement state (position, grounded, ...). Moved by <see cref="OpenGta2.Physics.CharacterMover"/>.
    /// </summary>
    public CharacterBody Body;

    public Ped(Vector3 position, float rotation, int remap)
    {
        Body = new CharacterBody(new System.Numerics.Vector3(position.X, position.Y, position.Z), radius: 0.15f);
        Rotation = rotation;
        Remap = remap;
    }

    public Vector3 Position
    {
        get => new(Body.Position.X, Body.Position.Y, Body.Position.Z);
        set => Body.Position = new System.Numerics.Vector3(value.X, value.Y, value.Z);
    }
    public float Rotation { get; set; }
    public int Remap { get; }

    public PedAnimation Animation
    {
        get => _animation;
        set
        {
            if (_animation == value) return;

            _animation = value;
            AnimationFrame = 0;
        }
    }

    public int AnimationFrame { get; private set; }

    private int MaxAnimationFrame =>
        Animation switch
        {
            PedAnimation.Walking => 8,
            PedAnimation.Idle => 12,
            PedAnimation.Drowning => 3,
            _ => 0,
        };

    /// <summary>
    /// Sprite number of the first frame of the animation, relative to the start of the regular ped frames (158).
    /// The drowning frames come before them in the style file.
    /// </summary>
    public int AnimationBase =>
        Animation switch
        {
            PedAnimation.Walking => 8,
            PedAnimation.Idle => 53,
            PedAnimation.Drowning => -10, // sprites 148-150: the ped shrinks to nothing (see docs/ped-sprites.md; unverified)
            _ => 0,
        };

    private float AnimationFrameTime =>
        Animation switch
        {
            PedAnimation.Walking => 0.06f,
            PedAnimation.Idle => 0.2f,
            PedAnimation.Drowning => 0.25f,
            _ => 0,
        };

    /// <summary>
    /// How long the drowning takes, from the splash until the ped is gone and can be respawned.
    /// </summary>
    public const float DrowningDuration = 1.5f;

    private const float DrowningVisibleDuration = 1f;

    public bool IsDrowning { get; private set; }

    /// <summary>
    /// The drowning animation has finished.
    /// </summary>
    public bool DrownedFinished => IsDrowning && _drowningTime >= DrowningDuration;

    /// <summary>
    /// False once the ped has sunk out of sight.
    /// </summary>
    public bool IsVisible => !IsDrowning || _drowningTime < DrowningVisibleDuration;

    public void Drown()
    {
        if (IsDrowning) return;

        IsDrowning = true;
        _drowningTime = 0;
        Animation = PedAnimation.Drowning;
    }

    /// <summary>
    /// Brings the ped back after drowning.
    /// </summary>
    public void Revive()
    {
        IsDrowning = false;
        _drowningTime = 0;
        Animation = PedAnimation.Idle;
    }

    public void UpdateAnimation(float deltaTime)
    {
        if (IsDrowning) _drowningTime += deltaTime;

        _animationTime += deltaTime;

        var frameTime = AnimationFrameTime;
        if (_animationTime > frameTime)
        {
            _animationTime -= frameTime;
            AnimationFrame++;

            if (AnimationFrame >= MaxAnimationFrame)
                // drowning plays once and stays on its last frame
                AnimationFrame = Animation == PedAnimation.Drowning ? MaxAnimationFrame - 1 : 0;
        }

    }

    private float _animationTime;
    private float _drowningTime;
}

public enum PedAnimation
{
    Idle,
    Walking,
    Drowning
}