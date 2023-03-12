using Microsoft.Xna.Framework;

namespace OpenGta2.Client.Components;

public abstract class GtaComponent : GameComponent
{
    protected GtaComponent(GtaGame game) : base(game)
    {
        Game = game;
    }

    protected new GtaGame Game { get; }
}