using Microsoft.Xna.Framework;

namespace OpenGta2.Client.Peds;

public class Ped
{
    private PedAnimation _animation;

    public Ped(Vector3 position, float rotation, int remap)
    {
        Position = position;
        Rotation = rotation;
        Remap = remap;
    }

    public Vector3 Position { get; set; }
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