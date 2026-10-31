using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenGta2.Client.Assets.Effects;
using OpenGta2.Client.Levels;
using OpenGta2.Client.Rendering;
using OpenGta2.Client.Utilities;
using OpenGta2.GameData.Style;

namespace OpenGta2.Client.Diagnostics.Screens;

/// <summary>
/// Debug tool for finding out what the sprites in the style file are.
/// </summary>
/// <remarks>
/// It starts as a grid of 8 x 8 sprites with their numbers; PageUp/PageDown flip pages and Home/End
/// jump to the first/last page. F5 switches to a single large sprite in the middle of the screen, where PageUp/PageDown
/// step by 1, Home/End by 10 and Enter plays on from the current sprite at 10 sprites per second.
/// F6 switches between car, ped, code object, map object, user and font sprites. F7 toggles additive blending
/// (some effect sprites only look right that way). F8 cycles through the remap (recolour) palettes of the current kind:
/// the original game recolours sprites such as explosions, so a sprite can look completely different in another remap.
/// The numbers are the ones <c>GetSpriteTexture</c> takes.
/// </remarks>
public class SpriteDebugScreen : IDebugScreen
{
    private const float PlaySpeed = 10;
    private const int GridSize = 8;
    private const int SpritesPerPage = GridSize * GridSize;

    private readonly GtaGame _game;
    private readonly Camera _camera;
    private readonly Controls _controls;
    private readonly LevelProvider _levelProvider;
    private IndexBuffer? _indices;
    private VertexBuffer? _vertices;
    private WorldSpriteEffect? _effect;
    private SpriteBatch? _spriteBatch;
    private SpriteFont? _font;
    private Texture2D? _pixel;

    private bool _grid = true;
    private bool _additive;
    private bool _playing;
    private float _playTime;
    private SpriteKind _kind = SpriteKind.CodeObj;
    private int _number;
    private int _remap = -1;

    public SpriteDebugScreen(GtaGame game, Camera camera, Controls controls, LevelProvider levelProvider)
    {
        _game = game;
        _camera = camera;
        _controls = controls;
        _levelProvider = levelProvider;
    }

    public string Name => "Sprites";

    public IReadOnlyList<DebugKey> HelpKeys { get; } = new DebugKey[]
    {
        new("PgUp / PgDn", "next / previous page (grid) or sprite (single)"),
        new("Home / End", "first / last page (grid) or +-10 sprites (single)"),
        new("F5", "grid / single sprite"),
        new("F6", "sprite kind (car, ped, code obj, map obj, user, font)"),
        new("F7", "additive blending"),
        new("F8", "next remap palette"),
        new("Enter", "play on from the sprite (single)"),
    };

    private GraphicsDevice GraphicsDevice => _game.GraphicsDevice;

    public void LoadContent()
    {
        _effect = _game.AssetManager.CreateWorldSpriteEffect();
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = _game.AssetManager.GetDebugFont();

        // a single white pixel to draw the cell backgrounds with
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _indices = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);
        _vertices = new VertexBuffer(GraphicsDevice, typeof(VertexPositionSprite), 4, BufferUsage.WriteOnly);
        _vertices.SetData(new VertexPositionSprite[]
        {
            new(new Vector3(-0.5f, -0.5f, 0), new Vector2(0, 0)),
            new(new Vector3(0.5f, -0.5f, 0), new Vector2(1, 0)),
            new(new Vector3(-0.5f, 0.5f, 0), new Vector2(0, 1)),
            new(new Vector3(0.5f, 0.5f, 0), new Vector2(1, 1))
        });
        _indices.SetData(new short[] { 0, 1, 2, 2, 1, 3 });
    }

    private int Count => _kind switch
    {
        SpriteKind.Car => _levelProvider.Style.SpriteBases.Car,
        SpriteKind.Ped => _levelProvider.Style.SpriteBases.Ped,
        SpriteKind.CodeObj => _levelProvider.Style.SpriteBases.CodeObj,
        SpriteKind.mapObj => _levelProvider.Style.SpriteBases.MapObj,
        SpriteKind.User => _levelProvider.Style.SpriteBases.User,
        _ => _levelProvider.Style.SpriteBases.Font
    };

    private string RemapText => _remap < 0 ? "default palette" : $"remap {_remap + 1}/{_levelProvider.Style.PaletteBase.GetRemap(_kind)}";

    private int PageCount => (Count + SpritesPerPage - 1) / SpritesPerPage;

    public void Opened()
    {
    }

    public void Closed() => DiagnosticValues.Set("Sprite", "off");

    public void Update(GameTime gameTime)
    {
        if (_controls.IsKeyDown(Keys.F6))
        {
            _kind = _kind == SpriteKind.Font ? SpriteKind.Car : _kind + 1;
            _number = 0;
            _remap = -1;
        }

        if (_controls.IsKeyDown(Keys.F7)) _additive = !_additive;
        if (_controls.IsKeyDown(Keys.F8))
        {
            // -1 is the sprite's own palette, then each of the remap palettes of this kind
            _remap = _remap + 1 >= _levelProvider.Style.PaletteBase.GetRemap(_kind) ? -1 : _remap + 1;
        }
        if (_controls.IsKeyDown(Keys.F5))
        {
            _grid = !_grid;
            _playing = false;
        }

        if (_grid)
        {
            UpdateGrid();
        }
        else
        {
            UpdateSingle(gameTime);
        }

        _number = Math.Clamp(_number, 0, Count - 1);
    }

    private void UpdateGrid()
    {
        // _number is the first sprite of the page
        var page = _number / SpritesPerPage;

        if (_controls.IsKeyDown(Keys.PageUp)) page++;
        if (_controls.IsKeyDown(Keys.PageDown)) page--;
        if (_controls.IsKeyDown(Keys.Home)) page = 0;
        if (_controls.IsKeyDown(Keys.End)) page = PageCount - 1;

        page = Math.Clamp(page, 0, PageCount - 1);
        _number = page * SpritesPerPage;
    }

    private void UpdateSingle(GameTime gameTime)
    {
        if (_controls.IsKeyDown(Keys.Enter)) _playing = !_playing;

        var step = 0;
        if (_controls.IsKeyDown(Keys.PageUp)) step = 1;
        if (_controls.IsKeyDown(Keys.PageDown)) step = -1;
        if (_controls.IsKeyDown(Keys.Home)) step = 10;
        if (_controls.IsKeyDown(Keys.End)) step = -10;

        if (step != 0)
        {
            _playing = false;
            _number += step;
        }

        if (_playing)
        {
            _playTime += gameTime.GetDelta();
            while (_playTime >= 1 / PlaySpeed)
            {
                _playTime -= 1 / PlaySpeed;
                _number++;
            }
        }

        if (_playing && _number >= Count - 1) _playing = false;
    }

    public void Draw(GameTime gameTime)
    {
        if (_grid)
        {
            DrawGrid();
        }
        else
        {
            DrawSingle();
        }
    }

    private void DrawGrid()
    {
        var first = _number / SpritesPerPage * SpritesPerPage;
        var last = Math.Min(first + SpritesPerPage, Count) - 1;

        DiagnosticValues.Set("Sprite", string.Create(CultureInfo.InvariantCulture,
            $"{_kind} grid page {first / SpritesPerPage + 1}/{PageCount} (sprites {first}-{last}) {(_additive ? "additive" : "normal")} {RemapText}"));

        var viewport = GraphicsDevice.Viewport;
        var margin = 10;
        var size = Math.Min(viewport.Width, viewport.Height) - 2 * margin;
        var cell = size / GridSize;
        var originX = (viewport.Width - cell * GridSize) / 2;
        var originY = (viewport.Height - cell * GridSize) / 2;

        // draw the background, then the sprites (with the chosen blending), then the numbers
        _spriteBatch!.Begin(blendState: BlendState.NonPremultiplied, samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_pixel, new Rectangle(originX - 4, originY - 4, cell * GridSize + 8, cell * GridSize + 8), new Color(0, 0, 0, 200));
        for (var i = 0; i < SpritesPerPage; i++)
        {
            var cellRect = new Rectangle(originX + i % GridSize * cell, originY + i / GridSize * cell, cell - 2, cell - 2);
            _spriteBatch.Draw(_pixel, cellRect, new Color(60, 80, 60, 255));
        }

        _spriteBatch.End();

        _spriteBatch.Begin(blendState: _additive ? BlendState.Additive : BlendState.NonPremultiplied, samplerState: SamplerState.PointClamp);
        for (var i = 0; i < SpritesPerPage && first + i <= last; i++)
        {
            var texture = _levelProvider.Textures.GetSpriteTexture(_kind, (ushort)(first + i), _remap);

            // as large as fits in the cell, whole multiples only
            var scale = Math.Clamp((cell - 22) / Math.Max(texture.Width, texture.Height), 1, 4);
            var x = originX + i % GridSize * cell + (cell - texture.Width * scale) / 2;
            var y = originY + i / GridSize * cell + 14 + (cell - 14 - texture.Height * scale) / 2;
            _spriteBatch.Draw(texture, new Rectangle(x, y, texture.Width * scale, texture.Height * scale), Color.White);
        }

        _spriteBatch.End();

        _spriteBatch.Begin(blendState: BlendState.NonPremultiplied);
        for (var i = 0; i < SpritesPerPage && first + i <= last; i++)
        {
            var texture = _levelProvider.Textures.GetSpriteTexture(_kind, (ushort)(first + i), _remap);
            var label = string.Create(CultureInfo.InvariantCulture, $"{first + i} {texture.Width}x{texture.Height}");
            _spriteBatch.DrawString(_font, label, new Vector2(originX + i % GridSize * cell + 3, originY + i / GridSize * cell + 1), Color.White);
        }

        _spriteBatch.End();

        // reset states changed by the sprite batch
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
    }

    private void DrawSingle()
    {
        var texture = _levelProvider.Textures.GetSpriteTexture(_kind, (ushort)_number, _remap);
        DiagnosticValues.Set("Sprite", string.Create(CultureInfo.InvariantCulture,
            $"{_kind} {_number}/{Count - 1} {texture.Width}x{texture.Height} {(_additive ? "additive" : "normal")} {RemapText}{(_playing ? " playing" : "")}"));

        var blend = GraphicsDevice.BlendState;
        var depth = GraphicsDevice.DepthStencilState;
        GraphicsDevice.BlendState = _additive ? BlendState.Additive : BlendState.NonPremultiplied;
        GraphicsDevice.DepthStencilState = DepthStencilState.None;

        GraphicsDevice.SetVertexBuffer(_vertices);
        GraphicsDevice.Indices = _indices;

        // in the middle of the screen, at about the height of the ground when the camera follows the player
        var position = new Vector3(_camera.Position.X, _camera.Position.Y, _camera.Position.Z - 7.5f);
        var world = Matrix.CreateScale(texture.Width / 64f, texture.Height / 64f, 1) * Matrix.CreateTranslation(position);

        _effect!.Texture = texture;
        _effect.Color = Color.White;
        _effect.TransformMatrix = world * _camera.ViewMatrix * _camera.Projection;
        _effect.CurrentTechnique.Passes[0].Apply();
        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);

        GraphicsDevice.BlendState = blend;
        GraphicsDevice.DepthStencilState = depth;
    }
}
