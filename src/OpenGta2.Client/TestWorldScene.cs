using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using OpenGta2.Data.Map;
using OpenGta2.Data.Riff;

namespace OpenGta2.Client;

public class TestWorldScene : Scene
{
    private Map? _map;

    public TestWorldScene(GtaGame game) : base(game)
    {
    }

    public Camera Camera { get; } = new();

    protected Map Map => _map ?? throw new InvalidOperationException();

    public override void Update(GameTime gameTime)
    {
        // Basic camera controls for testing
        var kb = Keyboard.GetState();
        var cameraInput = Vector3.Zero;

        if (kb.IsKeyDown(Keys.Right))
        {
            cameraInput += Vector3.Right;
        }

        if (kb.IsKeyDown(Keys.Left))
        {
            cameraInput += Vector3.Left;
        }
        
        if (kb.IsKeyDown(Keys.Up))
        {
            cameraInput += Vector3.Up;
        }

        if (kb.IsKeyDown(Keys.Down))
        {
            cameraInput += Vector3.Down;
        }
        if (kb.IsKeyDown(Keys.PageUp))
        {
            cameraInput += Vector3.Forward;
        }
        if (kb.IsKeyDown(Keys.PageDown))
        {
            cameraInput += Vector3.Backward;
        }

        Camera.Position += cameraInput * gameTime.GetDelta() * (Camera.Position.Z * 0.4f);
        Camera.Frustum.Matrix = Camera.ViewMatrix * Game.Projection;

        base.Update(gameTime);
    }

    public override void Initialize()
    {
        // Loading should probably happen in a loading scene.
        using var stream = TestGamePath.OpenFile("data/bil.gmp");
        var riffReader = new RiffReader(stream);
        var mapreader = new MapReader(riffReader);

        _map = mapreader.Read();

        Game.Components.Add(new MapComponent(Game, Camera, Map));

        base.Initialize();
    }
}