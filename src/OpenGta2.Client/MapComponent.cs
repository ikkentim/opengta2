using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenGta2.Client.Effects;
using OpenGta2.Data.Style;

namespace OpenGta2.Client;

public class MapComponent : DrawableGameComponent
{
    private readonly GtaGame _game;
    private readonly Camera _camera;
    private readonly LevelProvider _levelProvider;
    private Texture2D? _tilesTexture;
    private BlockFaceEffect? _blockFaceEffect;

    public MapComponent(GtaGame game, Camera camera) : base(game)
    {
        _game = game;
        _camera = camera;

        _levelProvider = game.Services.GetService<LevelProvider>();
    }

    protected override void LoadContent()
    {
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
        var style = _levelProvider.Style!;
        var tileCount = style.Tiles.Count;

        _tilesTexture = new Texture2D(GraphicsDevice, Tiles.TileWidth, Tiles.TileHeight, false, SurfaceFormat.Color,
            tileCount);

        // style can contain up to 992 tiles, each tile is 64x64 pixels.
        var tileData = new uint[Tiles.TileWidth * Tiles.TileHeight];

        for (ushort tileNumber = 0; tileNumber < tileCount; tileNumber++)
        {
            // don't need to add a base for virtual palette number - base for tiles is always 0.
            var physicalPaletteNumber = style.PaletteIndex.PhysPalette[tileNumber];
            var palette = style.PhysicsalPalette.GetPalette(physicalPaletteNumber);

            for (var y = 0; y < Tiles.TileHeight; y++)
            {
                var tileSlice = style.Tiles.GetTileSlice(tileNumber, y);
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

    public override void Update(GameTime gameTime)
    {
        _levelProvider.Update(_camera);
        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        _blockFaceEffect!.View = _camera.ViewMatrix;
        _blockFaceEffect.Projection = _camera.Projection;
        // _game.GraphicsDevice.BlendState = BlendState.AlphaBlend; // TODO: handle alpha in a shader

        foreach (var chunk in _levelProvider.GetRenderableChunks())
        {
            _game.GraphicsDevice.Indices = chunk.Indices;
            _game.GraphicsDevice.SetVertexBuffer(chunk.Vertices);

            _blockFaceEffect.World = chunk.Translation;

            // TODO: Should render flat tiles in a separate render pass to allow for transparency (instead of clipping)
            foreach (var pass in _blockFaceEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, chunk.PrimitiveCount);
            }
        }

        base.Draw(gameTime);
    }
}