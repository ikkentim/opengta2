using OpenGta2.Client.Assets;
using OpenGta2.Client.Components;

namespace OpenGta2.Client.Scenes
{
    public class IntroScene : Scene
    {
        private readonly Scene _nextScene;

        public IntroScene(GtaGame game, Scene nextScene) : base(game)
        {
            _nextScene = nextScene;
        }

        public override void Initialize()
        {
            Game.Components.Add(new IntroComponent(Game, _nextScene));
        }
    }
}
