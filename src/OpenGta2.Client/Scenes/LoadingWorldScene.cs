using OpenGta2.Client.Levels;

namespace OpenGta2.Client.Scenes;

public class LoadingWorldScene : Scene
{
    private readonly string _map;
    private readonly string _style;
    private readonly Scene _nextScene;

    public LoadingWorldScene(GtaGame game, string map, string style, Scene nextScene) : base(game)
    {
        _map = map;
        _style = style;
        _nextScene = nextScene;
    }

    public override void Initialize()
    {
        var levelProvider = Game.Services.GetService<LevelProvider>();
        if (levelProvider == null)
        {
            levelProvider = new LevelProvider(Game.GraphicsDevice);
            Game.Services.AddService(levelProvider);
        }

        levelProvider.LoadLevel(_map, _style);

        Game.ActivateScene(_nextScene);
    }
}