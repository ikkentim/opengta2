using Microsoft.Xna.Framework;

namespace OpenGta2.Client;

public class Camera
{
    public Vector3 Position { get; set; } = new(0, 0, 200);

    public Matrix ViewMatrix => Matrix.CreateLookAt(Position, Position + Vector3.Forward, Vector3.Up);
    
    public BoundingFrustum Frustum { get; } = new(Matrix.Identity);
}