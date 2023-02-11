using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenGta2.Data.Map;
using OpenGta2.Data.Style;

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
    private BlockFaceEffect? _blockFaceEffect;

    public MapComponent(GtaGame game, Camera camera, Map map, Style style) : base(game)
    {
        _game = game;
        _camera = camera;
        _map = map;
        _style = style;
    }

    protected override void LoadContent()
    {
        vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTile), 4 * 5, BufferUsage.WriteOnly);
        indexBuffer = new IndexBuffer(GraphicsDevice, typeof(short), 6 * 5, BufferUsage.WriteOnly);

        _blockFaceEffect = _game.Services.GetService<AssetManager>()
            .CreateBlockFaceEffect();
        
        CreateTilesTexture();
        
        _blockFaceEffect.Tiles = _tilesTexture!;
    }

    private void CreateTilesTexture()
    {
        // TODO: Move texture generation somewhere else, where we can also generate textures for the spritemaps.
        // TODO: Switch to loading each page as a texture in the texture array instead of each separate tile.
        // This would load more efficiently and makes no difference in rendering.
        var tileCount = _style.Tiles.Count;

        _tilesTexture = new Texture2D(GraphicsDevice, Tiles.TileWidth, Tiles.TileHeight, true, SurfaceFormat.Color,
            tileCount);

        // style can contain up to 992 tiles, each tile is 64x64 pixels.
        var tileData = new uint[Tiles.TileWidth * Tiles.TileHeight];

        for (ushort tileNumber = 0; tileNumber < tileCount; tileNumber++)
        {
            // don't need to add a base for virtual palette number - base for tiles is always 0.
            var physicalPaletteNumber = _style.PaletteIndex.PhysPalette[tileNumber];
            var palette = _style.PhysicsalPalette.GetPalette(physicalPaletteNumber);

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
            
            _tilesTexture.SetData(0, tileNumber, null, tileData, 0, tileData.Length);
        }
    }
    
    public override void Draw(GameTime gameTime)
    {
        _blockFaceEffect!.View = _camera.ViewMatrix;
        _blockFaceEffect.Projection = _game.Projection;
        _game.GraphicsDevice.Indices = indexBuffer;
        _game.GraphicsDevice.SetVertexBuffer(vertexBuffer);
        // _game.GraphicsDevice.BlendState = BlendState.AlphaBlend; // TODO: handle alpha in a shader

        var map = _map.CompressedMap;

        var maxX = _map.Width;
        var maxY = _map.Height;

        var vertices = new List<VertexPositionTile>();
        var indices = new List<short>();

        // TODO: Separate the map into segments. When a segment enters the view frustum, load the segment into vram
        // This would allow the map to stay in memory, and we wouldn't have to regenerate the vertices each draw call.
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

                    // TODO: Should render flat tiles in a separate render pass to allow for transparency (instead of clipping)
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