using OpenGta2.Client.Components;
using OpenGta2.Client.Diagnostics;
using OpenGta2.Client.Diagnostics.Screens;
using OpenGta2.Client.Peds;
using OpenGta2.Client.Utilities;

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
        Game.Services.ReplaceService(Camera);

        Game.Services.ReplaceService(new PedManager());
        Game.Services.ReplaceService(new DebugSettings());

        AddComponent<AudioTestComponent>();
        AddComponent<MapComponent>();
        AddComponent<SpriteTestComponent>();
        AddComponent<PlayerControllerComponent>();
        AddComponent<PedManagerComponent>();
        AddComponent<CameraComponent>();
        AddComponent<DebuggingDrawingComponent>();

        // the debug menu (F1) with its screens; add new IDebugScreen implementations here
        var debugMenu = new DebugMenuComponent(Game, Game.Services.GetService<Controls>());
        debugMenu.Add<WorldDebugScreen>();
        debugMenu.Add<SpriteDebugScreen>();
        debugMenu.Add<SoundDebugScreen>();
        Game.Components.Add(debugMenu);
    }
}