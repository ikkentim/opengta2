using Microsoft.Xna.Framework;

namespace OpenGta2.Client;

public class Camera
{
    public Vector3 Position { get; set; } = new(0, 0, 20);

    public Matrix ViewMatrix => Matrix.CreateLookAt(Position, Position - GtaVector.Skywards, GtaVector.Up);
    
    public BoundingFrustum Frustum { get; } = new(Matrix.Identity);
}