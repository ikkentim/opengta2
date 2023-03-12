using Microsoft.Xna.Framework;

namespace OpenGta2.Client.Components;

public abstract class DrawableGtaComponent : DrawableGameComponent
{
    protected DrawableGtaComponent(GtaGame game) : base(game)
    {
        Game = game;
    }

    protected new GtaGame Game { get; }
}