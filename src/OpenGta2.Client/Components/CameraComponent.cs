using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using OpenGta2.Client.Levels;
using OpenGta2.Client.Utilities;

namespace OpenGta2.Client.Components;

public class CameraComponent : GtaComponent
{
    private readonly Controls _controls;

    public CameraComponent(GtaGame game, Camera camera) : base(game)
    {
        Camera = camera;
        _controls = game.Services.GetService<Controls>();
    }
    
    private Camera Camera { get; }

    public override void Update(GameTime gameTime)
    {
        var cameraInput = GetCamControlsVec();

        if (cameraInput != Vector3.Zero)
        {
            Camera.Free();
            Camera.Position += cameraInput * 3 * gameTime.GetDelta() * (Camera.Position.Z * 0.4f);
        }

        if (_controls.IsKeyDown(Keys.NumPad0))
        {
            Camera.Unfree();
        }
        
        switch (Camera.Mode)
        {
            case CameraMode.AttachedToPed:
                Camera.Position = Camera.AttachedToPed!.Position + GtaVector.Skywards * 8;
                break;
        }
        
        Camera.Frustum.Matrix = Camera.ViewMatrix * Camera.ProjectionLhs;
    }

    private Vector3 GetCamControlsVec()
    {
        var cameraInput = Vector3.Zero;

        if (_controls.IsKeyPressed(Keys.NumPad6))
            cameraInput += GtaVector.Right;

        if (_controls.IsKeyPressed(Keys.NumPad4))
            cameraInput += GtaVector.Left;

        if (_controls.IsKeyPressed(Keys.NumPad8))
            cameraInput += GtaVector.Up;

        if (_controls.IsKeyPressed(Keys.NumPad2))
            cameraInput += GtaVector.Down;

        if (_controls.IsKeyPressed(Keys.NumPad7))
            cameraInput += GtaVector.Skywards;

        if (_controls.IsKeyPressed(Keys.NumPad9))
            cameraInput -= GtaVector.Skywards;

        return cameraInput;
    }
}