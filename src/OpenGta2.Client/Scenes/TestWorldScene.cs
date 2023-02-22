using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using OpenGta2.Client.Components;
using OpenGta2.Client.Levels;
using OpenGta2.Client.Utilities;

namespace OpenGta2.Client.Scenes;

public class TestWorldScene : Scene
{
    public TestWorldScene(GtaGame game) : base(game)
    {
        Camera = new Camera(game.Window);
    }

    public Camera Camera { get; }

    public override void Update(GameTime gameTime)
    {
        UpdateCamera(gameTime);

        base.Update(gameTime);
    }

    private void UpdateCamera(GameTime gameTime)
    {
        // Basic camera controls for testing
        var kb = Keyboard.GetState();
        var cameraInput = Vector3.Zero;

        if (kb.IsKeyDown(Keys.Right) || kb.IsKeyDown(Keys.D))
            cameraInput += GtaVector.Right;

        if (kb.IsKeyDown(Keys.Left) || kb.IsKeyDown(Keys.A))
            cameraInput += GtaVector.Left;

        if (kb.IsKeyDown(Keys.Up) || kb.IsKeyDown(Keys.W))
            cameraInput += GtaVector.Up;

        if (kb.IsKeyDown(Keys.Down) || kb.IsKeyDown(Keys.S))
            cameraInput += GtaVector.Down;
        if (kb.IsKeyDown(Keys.PageUp))
            cameraInput += GtaVector.Skywards;
        if (kb.IsKeyDown(Keys.PageDown))
            cameraInput -= GtaVector.Skywards;
        
        if (kb.IsKeyDown(Keys.LeftControl))
            cameraInput *= 2;

        if (kb.IsKeyDown(Keys.RightControl))
            cameraInput *= 2;

        Camera.Position += cameraInput * gameTime.GetDelta() * (Camera.Position.Z * 0.4f);
        Camera.Frustum.Matrix = Camera.ViewMatrix * Camera.ProjectionLhs;
    }

    public override void Initialize()
    {
        Game.Components.Add(new MapComponent(Game, Camera));
    }
}