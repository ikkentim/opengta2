using System.Numerics;

namespace OpenGta2.Physics;

/// <summary>
/// Movement state of a walking character. <see cref="Position"/>.Z is the height of the character's feet.
/// </summary>
public struct CharacterBody
{
    public Vector3 Position;
    public float VelocityZ;
    public bool Grounded;

    /// <summary>
    /// Height the character was at when it last left the ground. Used to detect (and recover from) endless falls.
    /// </summary>
    public float FallStartZ;

    public float Radius;
    public float Height;

    /// <summary>
    /// Maximum height difference the character walks up without jumping.
    /// </summary>
    public float StepUp;

    /// <summary>
    /// Maximum height difference the character stays glued to the ground when walking down.
    /// </summary>
    public float StepDown;

    public CharacterBody(Vector3 position, float radius = 0.15f, float height = 0.6f, float stepUp = 0.35f, float stepDown = 0.35f)
    {
        Position = position;
        VelocityZ = 0;
        Grounded = false;
        FallStartZ = position.Z;
        Radius = radius;
        Height = height;
        StepUp = stepUp;
        StepDown = stepDown;
    }

    public Vector2 PositionXY
    {
        readonly get => new(Position.X, Position.Y);
        set => Position = new Vector3(value, Position.Z);
    }
}
