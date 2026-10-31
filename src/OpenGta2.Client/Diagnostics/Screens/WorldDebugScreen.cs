using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using OpenGta2.Client.Levels;

namespace OpenGta2.Client.Diagnostics.Screens;

/// <summary>
/// Switches for looking at the world: the collision boxes, the player speed and the speed of the tile animations.
/// </summary>
public class WorldDebugScreen : IDebugScreen
{
    private readonly Controls _controls;
    private readonly DebugSettings _settings;
    private readonly LevelProvider _levelProvider;

    public WorldDebugScreen(Controls controls, DebugSettings settings, LevelProvider levelProvider)
    {
        _controls = controls;
        _settings = settings;
        _levelProvider = levelProvider;
    }

    public string Name => "World and player";

    public IReadOnlyList<DebugKey> HelpKeys { get; } = new DebugKey[]
    {
        new("F5", "collision boxes on / off"),
        new("F6", "slow player (x0.1) on / off"),
        new("F7 / F8", "tile animations slower / faster (2 game frames per second)"),
        new("Numpad 5", "teleport the player to the highest point at the screen centre"),
    };

    public void LoadContent()
    {
    }

    public void Opened() => Show();

    public void Closed()
    {
    }

    public void Update(GameTime gameTime)
    {
        if (_controls.IsKeyDown(Keys.F5)) _settings.ShowHighlights = !_settings.ShowHighlights;
        if (_controls.IsKeyDown(Keys.F6)) _settings.SlowPlayer = !_settings.SlowPlayer;

        var animator = _levelProvider.TileAnimator;
        if (animator != null)
        {
            if (_controls.IsKeyDown(Keys.F7)) animator.GameFramesPerSecond = Math.Max(2, animator.GameFramesPerSecond - 2);
            if (_controls.IsKeyDown(Keys.F8)) animator.GameFramesPerSecond = Math.Min(120, animator.GameFramesPerSecond + 2);
        }

        Show();
    }

    public void Draw(GameTime gameTime)
    {
    }

    private void Show()
    {
        DiagnosticValues.Set("Debug", $"collision boxes {(_settings.ShowHighlights ? "on" : "off")}, slow player {(_settings.SlowPlayer ? "on" : "off")}");
    }
}
