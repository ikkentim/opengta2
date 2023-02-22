using Microsoft.Xna.Framework;

namespace OpenGta2.Client.Scenes;

public abstract class Scene : GameComponent
{
    protected Scene(GtaGame game) : base(game)
    {
        Game = game;
    }

    public new GtaGame Game { get; }
}