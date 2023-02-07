using Microsoft.Xna.Framework;

namespace OpenGta2.Client;

public class Camera
{
    public Vector3 Position { get; set; } = new(0, 0, 200);

    public Matrix ViewMatrix => Matrix.CreateLookAt(Position, Position - MapComponent.GtaVector.Skywards, MapComponent.GtaVector.Up);
    
    public BoundingFrustum Frustum { get; } = new(Matrix.Identity);
}