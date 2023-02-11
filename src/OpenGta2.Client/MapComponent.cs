using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenGta2.Data.Map;
using OpenGta2.Data.Style;
using SharpDX.Direct2D1.Effects;

namespace OpenGta2.Client;

public class MapComponent : DrawableGameComponent
{
    private readonly GtaGame _game;
    private readonly Camera _camera;
    private readonly Map _map;
    private readonly Style _style;
    private VertexBuffer? vertexBuffer;
    private IndexBuffer? indexBuffer;
    private Texture2D? _tilesTexture;
    private Texture2D? _tilesTexture2;
    private BlockFaceEffect? _blockFaceEffect;
    private BasicEffect? _basicEffect;

    public MapComponent(GtaGame game, Camera camera, Map map, Style style) : base(game)
    {
        _game = game;
        _camera = camera;
        _map = map;
        _style = style;
    }

    protected override void LoadContent()
    {
        // for now simple buffers for drawing a single face. will optimize this later.
        vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTile), 4 * 5, BufferUsage.WriteOnly);
        // vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), 4 * 5, BufferUsage.WriteOnly);

        indexBuffer = new IndexBuffer(GraphicsDevice, typeof(short), 6 * 5, BufferUsage.WriteOnly);
        _tilesTexture = new Texture2D(GraphicsDevice, 2048, 2048, false, SurfaceFormat.Color);
        _blockFaceEffect = _game.Services.GetService<AssetManager>()
            .CreateBlockFaceEffect();

        _basicEffect = new BasicEffect(GraphicsDevice);

        CreateTilesTexture();
        
        _blockFaceEffect.Tiles = _tilesTexture2!;
    }

    private void CreateTilesTexture()
    {
        var tileCount = _style.Tiles.Count;

        _tilesTexture2 = new Texture2D(GraphicsDevice, Tiles.TileWidth, Tiles.TileHeight, false, SurfaceFormat.Color,
            tileCount);

        // style can contain up to 992 tiles, each tile is 64x64 pixels. we're going to draw each tile
        // on a single 2048x2048 texture map. the texture map will provide room for 32x32 tiles.
        var tileData = new uint[Tiles.TileWidth * Tiles.TileHeight];

        for (ushort tileNumber = 0; tileNumber < tileCount; tileNumber++)
        {
            // don't need to add a base for virtual palette number - base for tiles is always 0.
            var physicalPaletteNumber = _style.PaletteIndex.PhysPalette[tileNumber];
            
            var palette = _style.PhysicsalPalette.GetPalette(physicalPaletteNumber);
            var colNum = tileNumber % 32;
            var rowNum = tileNumber / 32;

            for (var y = 0; y < Tiles.TileHeight; y++)
            {
                var tileSlice = _style.Tiles.GetTileSlice(tileNumber, y);
                for (var x = 0; x < Tiles.TileWidth; x++)
                {
                    var colorEntry = tileSlice[x]; // 0 is always transparent
                    tileData[y * Tiles.TileWidth + x] = colorEntry == 0
                        ? 0
                        : palette.GetColor(colorEntry)
                            .Argb;
                }
            }

            var textureRect = new Rectangle(colNum * Tiles.TileWidth, rowNum * Tiles.TileHeight, Tiles.TileWidth,
                Tiles.TileHeight);
            _tilesTexture!.SetData(0, textureRect, tileData, 0, tileData.Length);

            _tilesTexture2.SetData(0, tileNumber, null, tileData, 0, tileData.Length);
        }

        using var stream = File.OpenWrite($"debug{DateTime.Now:HHmmsss}.png");
        _tilesTexture!.SaveAsPng(stream, 2048, 2048);
    }
    
    public override void Draw(GameTime gameTime)
    {
        
        _basicEffect.TextureEnabled = true;
        _basicEffect.View = _camera.ViewMatrix;
        _basicEffect.Projection = _game.Projection;
        _basicEffect.LightingEnabled = false;
        _basicEffect.Texture = _tilesTexture;
        _basicEffect.FogEnabled = false;

        _blockFaceEffect!.View = _camera.ViewMatrix;
        _blockFaceEffect.Projection = _game.Projection;
        _game.GraphicsDevice.Indices = indexBuffer;
        _game.GraphicsDevice.SetVertexBuffer(vertexBuffer);
        // _game.GraphicsDevice.BlendState = BlendState.AlphaBlend; // TODO: handle alpha in a shader

        var map = _map.CompressedMap;

        var maxX = _map.Width;
        var maxY = _map.Height;

        var vertices = new List<VertexPositionTile>();
        // var vertices = new List<VertexPositionTexture>();
        var indices = new List<short>();

        for (var x = 0; x < maxX; x++)
        for (var y = 0; y < maxY; y++)
        {
            // simple column culling
            var colMin = new Vector2(x, y);
            var colMax = colMin + Vector2.One;
            if (!_camera.Frustum.Intersects(new BoundingBox(new Vector3(colMin, 0), new Vector3(colMax, 5))))
            {
                continue;
            }

            // read compressed map and render column
            var column = _map.GetColumn(x, y);

            for (var z = column.Offset; z < column.Height; z++)
            {
                vertices.Clear();
                indices.Clear();

                var blockNum = column.Blocks[z - column.Offset];
                ref var block = ref map.Blocks[blockNum];

                SlopeGenerator.Push(ref block, vertices, indices);

                if (indices.Count > 0)
                {
                    // TODO: don't use list so we have access to inner array
                    vertexBuffer!.SetData(vertices.ToArray());
                    indexBuffer!.SetData(indices.ToArray());

                    _blockFaceEffect.World = Matrix.CreateTranslation(new Vector3(x, y, z));
                    // _basicEffect.World = Matrix.CreateTranslation(new Vector3(x, y, z));

                    // foreach (var pass in _basicEffect.CurrentTechnique.Passes)
                    foreach (var pass in _blockFaceEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        _game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indices.Count / 3);
                    }
                }
            }
        }

        base.Draw(gameTime);
    }
}