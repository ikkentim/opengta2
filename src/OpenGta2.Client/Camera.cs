using Microsoft.Xna.Framework;
using OpenGta2.Client.Components;
using OpenGta2.Client.Levels;

namespace OpenGta2.Client;

public class Camera
{
    private readonly GameWindow _window;

    public Camera(GameWindow window)
    {
        _window = window;
    }

    public Vector3 Position { get; set; } = new(11.5f, 2.5f, 20);

    public Matrix ViewMatrix => Matrix.CreateLookAt(Position, Position - GtaVector.Skywards, GtaVector.Up);

    public Matrix Projection => GetProjection();

    public Matrix ProjectionLhs =>
        Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, // 90 fov
            _window.ClientBounds.Width / (float)_window.ClientBounds.Height, 0.1f, Position.Z + 1);


    public BoundingFrustum Frustum { get; } = new(Matrix.Identity);
    
    public Ped? AttachedToPed { get; private set; }

    public CameraMode Mode { get; private set; }

    public void Attach(Ped ped)
    {
        AttachedToPed = ped;
        Mode = CameraMode.AttachedToPed;
    }

    public void Free()
    {
        Mode = CameraMode.Free;
    }

    public void Unfree()
    {
        if (AttachedToPed != null)
        {
            Mode = CameraMode.AttachedToPed;
        }
    }

    private Matrix GetProjection()
    {
        var p = ProjectionLhs;

        // Invert matrix because DirectX is LHS and MonoGame is RHS
        p.M11 = -p.M11;
        p.M13 = -p.M13;

        return p;
    }
}