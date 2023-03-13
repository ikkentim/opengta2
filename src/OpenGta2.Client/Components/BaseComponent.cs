using Microsoft.Xna.Framework;

namespace OpenGta2.Client.Components;

public abstract class BaseComponent : GameComponent
{
    protected BaseComponent(GtaGame game) : base(game)
    {
        Game = game;
    }

    protected new GtaGame Game { get; }
}