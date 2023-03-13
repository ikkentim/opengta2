using Microsoft.Xna.Framework;
using OpenGta2.Client.Utilities;

namespace OpenGta2.Client.Scenes;

public abstract class Scene : GameComponent
{
    protected Scene(GtaGame game) : base(game)
    {
        Game = game;
    }

    public new GtaGame Game { get; }

    public void AddComponent<T>() where T : IGameComponent
    {
        var component = ComponentActivator<T>.Activate(Game);
        Game.Components.Add(component);
    }
}