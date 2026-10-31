using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenGta2.Client.Components;
using OpenGta2.Client.Utilities;

namespace OpenGta2.Client.Diagnostics;

/// <summary>
/// The debug menu. F1 opens and closes it, F2/F3 move up/down, F4 opens the selected <see cref="IDebugScreen"/>.
/// In a screen, F1 goes back to the menu. The keys of the open screen are always drawn at the bottom of the screen.
/// </summary>
public class DebugMenuComponent : BaseDrawableComponent
{
    private readonly Controls _controls;
    private readonly List<IDebugScreen> _screens = new();
    private SpriteBatch? _spriteBatch;
    private SpriteFont? _font;
    private Texture2D? _pixel;

    private bool _menuOpen;
    private int _selected;
    private IDebugScreen? _active;

    public DebugMenuComponent(GtaGame game, Controls controls) : base(game)
    {
        _controls = controls;
        DrawOrder = 900;
    }

    /// <summary>
    /// Adds a screen to the menu; it is created like a component, with its constructor arguments taken from the services.
    /// </summary>
    public void Add<T>() where T : class, IDebugScreen
    {
        _screens.Add(ComponentActivator<T>.Activate(Game));
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = Game.AssetManager.GetDebugFont();
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        foreach (var screen in _screens)
        {
            screen.LoadContent();
        }

        base.LoadContent();
    }

    public override void Update(GameTime gameTime)
    {
        if (_controls.IsKeyDown(Keys.F1))
        {
            if (_active != null)
            {
                CloseScreen();
            }
            else
            {
                _menuOpen = !_menuOpen;
            }

            return;
        }

        if (_active != null)
        {
            _active.Update(gameTime);
            return;
        }

        if (!_menuOpen || _screens.Count == 0) return;

        if (_controls.IsKeyDown(Keys.F2)) _selected = (_selected + _screens.Count - 1) % _screens.Count;
        if (_controls.IsKeyDown(Keys.F3)) _selected = (_selected + 1) % _screens.Count;

        if (_controls.IsKeyDown(Keys.F4))
        {
            _active = _screens[_selected];
            _active.Opened();
        }
    }

    private void CloseScreen()
    {
        _active?.Closed();
        _active = null;
    }

    public override void Draw(GameTime gameTime)
    {
        if (!_menuOpen && _active == null) return;

        _active?.Draw(gameTime);

        if (_active != null)
        {
            DrawKeys(_active.Name, _active.HelpKeys, "F1 back to the menu");
        }
        else
        {
            DrawMenu();
        }

        // reset states changed by the sprite batch
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
    }

    private void DrawMenu()
    {
        var keys = new[] { new DebugKey("F2 / F3", "up / down"), new DebugKey("F4", "open") };
        var lineHeight = (int)_font!.MeasureString("A").Y + 2;
        var width = 360;
        var height = (_screens.Count + 2) * lineHeight;
        var x = GraphicsDevice.Viewport.Width - width - 16;
        var y = 16;

        _spriteBatch!.Begin(blendState: BlendState.NonPremultiplied);
        _spriteBatch.Draw(_pixel, new Rectangle(x - 6, y - 6, width + 12, height + 12), new Color(0, 0, 0, 215));
        _spriteBatch.DrawString(_font, "Debug", new Vector2(x, y), Color.White);
        y += lineHeight * 2;

        for (var i = 0; i < _screens.Count; i++)
        {
            _spriteBatch.DrawString(_font, (i == _selected ? "> " : "  ") + _screens[i].Name, new Vector2(x, y), i == _selected ? Color.Yellow : Color.White);
            y += lineHeight;
        }

        _spriteBatch.End();

        DrawKeys("Debug menu", keys, "F1 close");
    }

    /// <summary>
    /// Draws the box with the keys of the screen at the bottom left.
    /// </summary>
    private void DrawKeys(string title, IReadOnlyList<DebugKey> keys, string back)
    {
        var lineHeight = (int)_font!.MeasureString("A").Y + 2;
        var keyWidth = 0f;
        var width = _font.MeasureString(title).X;
        foreach (var key in keys)
        {
            keyWidth = Math.Max(keyWidth, _font.MeasureString(key.Key).X);
        }

        foreach (var key in keys)
        {
            width = Math.Max(width, keyWidth + 16 + _font.MeasureString(key.Description).X);
        }

        width = Math.Max(width, keyWidth + 16 + _font.MeasureString(back).X);

        var viewport = GraphicsDevice.Viewport;
        var height = (keys.Count + 3) * lineHeight;
        var x = 16;
        var y = viewport.Height - height - 16;

        _spriteBatch!.Begin(blendState: BlendState.NonPremultiplied);
        _spriteBatch.Draw(_pixel, new Rectangle(x - 6, y - 6, (int)width + 12, height + 12), new Color(0, 0, 0, 215));
        _spriteBatch.DrawString(_font, title, new Vector2(x, y), Color.White);
        y += lineHeight * 2;

        foreach (var key in keys)
        {
            _spriteBatch.DrawString(_font, key.Key, new Vector2(x, y), Color.Yellow);
            _spriteBatch.DrawString(_font, key.Description, new Vector2(x + keyWidth + 16, y), Color.LightGray);
            y += lineHeight;
        }

        var f1 = back.Split(' ', 2);
        _spriteBatch.DrawString(_font, f1[0], new Vector2(x, y), Color.Yellow);
        _spriteBatch.DrawString(_font, f1[1], new Vector2(x + keyWidth + 16, y), Color.LightGray);
        _spriteBatch.End();
    }
}
