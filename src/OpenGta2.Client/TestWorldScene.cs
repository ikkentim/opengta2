using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using OpenGta2.Data.Map;
using OpenGta2.Data.Riff;
using OpenGta2.Data.Style;

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
        UpdateCamera(gameTime);

        base.Update(gameTime);
    }

    private void UpdateCamera(GameTime gameTime)
    {
        // Basic camera controls for testing
        var kb = Keyboard.GetState();
        var cameraInput = Vector3.Zero;

        if (kb.IsKeyDown(Keys.Right))
        {
            cameraInput += GtaVector.Right;
        }

        if (kb.IsKeyDown(Keys.Left))
        {
            cameraInput += GtaVector.Left;
        }
        
        if (kb.IsKeyDown(Keys.Up))
        {
            cameraInput += GtaVector.Up;
        }

        if (kb.IsKeyDown(Keys.Down))
        {
            cameraInput += GtaVector.Down;
        }
        if (kb.IsKeyDown(Keys.PageUp))
        {
            cameraInput += GtaVector.Skywards;
        }
        if (kb.IsKeyDown(Keys.PageDown))
        {
            cameraInput -= GtaVector.Skywards;
        }

        Camera.Position += cameraInput * gameTime.GetDelta() * (Camera.Position.Z * 0.4f);
        Camera.Frustum.Matrix = Camera.ViewMatrix * Game.ProjectionLhs;
    }

    public override void Initialize()
    {
        // Loading should probably happen in a loading scene.

        using var mapStream = TestGamePath.OpenFile("data/bil.gmp");
        using var mapRiffReader = new RiffReader(mapStream);
        var mapreader = new MapReader(mapRiffReader);

        using var styleStream = TestGamePath.OpenFile("data/bil.sty");
        using var styleRiffReader = new RiffReader(styleStream);
        var styleReader = new StyleReader(styleRiffReader);

        _map = mapreader.Read();
        var style = styleReader.Read();

        Game.Components.Add(new MapComponent(Game, Camera, Map, style));

        base.Initialize();
    }
}