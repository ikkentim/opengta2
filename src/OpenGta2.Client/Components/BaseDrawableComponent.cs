using Microsoft.Xna.Framework;

namespace OpenGta2.Client.Components;

public abstract class BaseDrawableComponent : DrawableGameComponent
{
    protected BaseDrawableComponent(GtaGame game) : base(game)
    {
        Game = game;
    }

    protected new GtaGame Game { get; }
}