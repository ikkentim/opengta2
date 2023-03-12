using OpenGta2.Client.Components;
using OpenGta2.Client.Diagnostics;

namespace OpenGta2.Client.Scenes;

public class TestWorldScene : Scene
{
    public TestWorldScene(GtaGame game) : base(game)
    {
        Camera = new Camera(game.Window);
    }

    public Camera Camera { get; }
    
    public override void Initialize()
    {
        Game.Components.Add(new MapComponent(Game, Camera));
        Game.Components.Add(new SpriteTestComponent(Game));
        Game.Components.Add(new PedManagerComponent(Game, Camera));
        Game.Components.Add(new CameraComponent(Game, Camera));
        Game.Components.Add(new DebuggingDrawingComponent(Game));

    }
}