using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenGta2.Client.Levels;

namespace OpenGta2.Client.Diagnostics.Screens;

/// <summary>
/// Debug tool for looking at the tiles (the 64 x 64 surface textures of blocks) of the style file, as they are in the tile texture
/// the map is drawn with (so animated tiles show their current frame).
/// </summary>
/// <remarks>
/// Starts as a grid of 8 x 8 tiles with their numbers. PageUp/PageDown flip pages, Home/End jump to the first/last page.
/// F5 switches to a single tile at 8 times its size, where PageUp/PageDown step by 1 and Home/End by 10. Transparent pixels show
/// the bright magenta background, so the shape of the opaque parts is clear. F6 draws a texel grid over the single tile.
/// </remarks>
public class TileDebugScreen : IDebugScreen
{
    private const int GridSize = 8;
    private const int TilesPerPage = GridSize * GridSize;
    private const int TileSize = 64;
    private const int SingleScale = 8;

    private readonly GtaGame _game;
    private readonly Controls _controls;
    private readonly LevelProvider _levelProvider;
    private readonly Texture2D?[] _textures = new Texture2D?[TilesPerPage];
    private readonly uint[] _buffer = new uint[TileSize * TileSize];
    private SpriteBatch? _spriteBatch;
    private SpriteFont? _font;
    private Texture2D? _pixel;

    private bool _grid = true;
    private bool _texelGrid;
    private int _number;

    public TileDebugScreen(GtaGame game, Controls controls, LevelProvider levelProvider)
    {
        _game = game;
        _controls = controls;
        _levelProvider = levelProvider;
    }

    public string Name => "Tiles";

    public IReadOnlyList<DebugKey> HelpKeys { get; } = new DebugKey[]
    {
        new("PgUp / PgDn", "next / previous page (grid) or tile (single)"),
        new("Home / End", "first / last page (grid) or +-10 tiles (single)"),
        new("F5", "grid / single tile (8x)"),
        new("F6", "texel grid on the single tile"),
    };

    private GraphicsDevice GraphicsDevice => _game.GraphicsDevice;

    private int Count => _levelProvider.Style.Tiles.TileCount;

    private int PageCount => (Count + TilesPerPage - 1) / TilesPerPage;

    public void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = _game.AssetManager.GetDebugFont();
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        for (var i = 0; i < TilesPerPage; i++)
        {
            _textures[i] = new Texture2D(GraphicsDevice, TileSize, TileSize);
        }
    }

    public void Opened()
    {
    }

    public void Closed() => DiagnosticValues.Set("Tile", "off");

    public void Update(GameTime gameTime)
    {
        if (_controls.IsKeyDown(Keys.F5)) _grid = !_grid;
        if (_controls.IsKeyDown(Keys.F6)) _texelGrid = !_texelGrid;

        if (_grid)
        {
            var page = _number / TilesPerPage;
            if (_controls.IsKeyDown(Keys.PageUp)) page++;
            if (_controls.IsKeyDown(Keys.PageDown)) page--;
            if (_controls.IsKeyDown(Keys.Home)) page = 0;
            if (_controls.IsKeyDown(Keys.End)) page = PageCount - 1;

            _number = Math.Clamp(page, 0, PageCount - 1) * TilesPerPage;
        }
        else
        {
            var step = 0;
            if (_controls.IsKeyDown(Keys.PageUp)) step = 1;
            if (_controls.IsKeyDown(Keys.PageDown)) step = -1;
            if (_controls.IsKeyDown(Keys.Home)) step = 10;
            if (_controls.IsKeyDown(Keys.End)) step = -10;

            _number = Math.Clamp(_number + step, 0, Count - 1);
        }
    }

    /// <summary>
    /// Copies the current pixels of a tile from the tile texture the map uses into the texture of the slot.
    /// Transparent pixels are made magenta.
    /// </summary>
    private Texture2D Load(int slot, int tile)
    {
        _levelProvider.Textures.TilesTexture.GetData(0, tile, null, _buffer, 0, _buffer.Length);

        for (var i = 0; i < _buffer.Length; i++)
        {
            // texture data is ABGR in memory order R, G, B, A: alpha is the top byte
            if (_buffer[i] >> 24 == 0) _buffer[i] = 0xFFFF00FF;
        }

        var texture = _textures[slot]!;
        texture.SetData(_buffer);
        return texture;
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

        // reset states changed by the sprite batch
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
    }

    private void DrawGrid()
    {
        var first = _number / TilesPerPage * TilesPerPage;
        var last = Math.Min(first + TilesPerPage, Count) - 1;

        DiagnosticValues.Set("Tile", string.Create(CultureInfo.InvariantCulture, $"grid page {first / TilesPerPage + 1}/{PageCount} (tiles {first}-{last})"));

        var viewport = GraphicsDevice.Viewport;
        var margin = 10;
        var size = Math.Min(viewport.Width, viewport.Height) - 2 * margin;
        var cell = size / GridSize;
        var originX = (viewport.Width - cell * GridSize) / 2;
        var originY = (viewport.Height - cell * GridSize) / 2;

        _spriteBatch!.Begin(blendState: BlendState.Opaque, samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_pixel, new Rectangle(originX - 4, originY - 4, cell * GridSize + 8, cell * GridSize + 8), Color.Black);
        for (var i = 0; i < TilesPerPage && first + i <= last; i++)
        {
            var texture = Load(i, first + i);
            var x = originX + i % GridSize * cell;
            var y = originY + i / GridSize * cell;
            _spriteBatch.Draw(texture, new Rectangle(x, y, cell - 2, cell - 2), Color.White);
        }

        _spriteBatch.End();

        _spriteBatch.Begin(blendState: BlendState.NonPremultiplied);
        for (var i = 0; i < TilesPerPage && first + i <= last; i++)
        {
            var x = originX + i % GridSize * cell;
            var y = originY + i / GridSize * cell;
            var label = (first + i).ToString(CultureInfo.InvariantCulture);
            var labelSize = _font!.MeasureString(label);

            // readable on any tile
            _spriteBatch.Draw(_pixel, new Rectangle(x + 1, y + 1, (int)labelSize.X + 4, (int)labelSize.Y), new Color(0, 0, 0, 170));
            _spriteBatch.DrawString(_font, label, new Vector2(x + 3, y + 1), Color.White);
        }

        _spriteBatch.End();
    }

    private void DrawSingle()
    {
        DiagnosticValues.Set("Tile", string.Create(CultureInfo.InvariantCulture, $"tile {_number}/{Count - 1}"));

        var texture = Load(0, _number);
        var viewport = GraphicsDevice.Viewport;
        var size = TileSize * SingleScale;
        var x = (viewport.Width - size) / 2;
        var y = (viewport.Height - size) / 2;

        _spriteBatch!.Begin(blendState: BlendState.Opaque, samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_pixel, new Rectangle(x - 4, y - 4, size + 8, size + 8), Color.Black);
        _spriteBatch.Draw(texture, new Rectangle(x, y, size, size), Color.White);
        _spriteBatch.End();

        _spriteBatch.Begin(blendState: BlendState.NonPremultiplied);
        if (_texelGrid)
        {
            for (var i = 0; i <= TileSize; i += 1)
            {
                // every 8th line is brighter
                var color = i % 8 == 0 ? new Color(255, 255, 255, 110) : new Color(255, 255, 255, 35);
                _spriteBatch.Draw(_pixel, new Rectangle(x + i * SingleScale, y, 1, size), color);
                _spriteBatch.Draw(_pixel, new Rectangle(x, y + i * SingleScale, size, 1), color);
            }
        }

        _spriteBatch.DrawString(_font, $"tile {_number}", new Vector2(x, y - 24), Color.White);
        _spriteBatch.End();
    }
}
