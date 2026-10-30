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
            _ => 0,
        };

    public int AnimationBase =>
        Animation switch
        {
            PedAnimation.Walking => 8,
            PedAnimation.Idle => 53,
            _ => 0,
        };

    private float AnimationFrameTime =>
        Animation switch
        {
            PedAnimation.Walking => 0.06f,
            PedAnimation.Idle => 0.2f,
            _ => 0,
        };

    public void UpdateAnimation(float deltaTime)
    {
        _animationTime += deltaTime;

        var frameTime = AnimationFrameTime;
        if (_animationTime > frameTime)
        {
            _animationTime -= frameTime;
            AnimationFrame++;

            if (AnimationFrame >= MaxAnimationFrame)
                AnimationFrame = 0;
        }

    }

    private float _animationTime;
}

public enum PedAnimation
{
    Idle,
    Walking
}