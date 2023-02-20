using System;
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
        GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;
        
        Span<Light> lights = stackalloc Light[BlockFaceEffect.MaxLights];

        _blockFaceEffect!.View = _camera.ViewMatrix;
        _blockFaceEffect.Projection = _camera.Projection;
        _blockFaceEffect.Flat = false;

        foreach (var chunk in _levelProvider.GetRenderableChunks())
        {
            if (chunk.SolidPrimitiveCount == 0) continue;

            _game.GraphicsDevice.Indices = chunk.Indices;
            _game.GraphicsDevice.SetVertexBuffer(chunk.Vertices);
            _blockFaceEffect.World = chunk.Translation;
            
            var chunkLights = CollectLights(lights, chunk.ChunkLocation);
            _blockFaceEffect.SetLights(chunkLights);

            foreach (var pass in _blockFaceEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, chunk.SolidPrimitiveCount);
            }
        }
        
        _blockFaceEffect.Flat = true;

        foreach (var chunk in _levelProvider.GetRenderableChunks())
        {
            if (chunk.FlatPrimitiveCount == 0) continue;

            _game.GraphicsDevice.Indices = chunk.Indices;
            _game.GraphicsDevice.SetVertexBuffer(chunk.Vertices);
            _blockFaceEffect.World = chunk.Translation;

            var chunkLights = CollectLights(lights, chunk.ChunkLocation);
            _blockFaceEffect.SetLights(chunkLights);

            foreach (var pass in _blockFaceEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, chunk.FlatIndexOffset, chunk.FlatPrimitiveCount);
            }
        }


        base.Draw(gameTime);
    }

    private Span<Light> CollectLights(Span<Light> buffer, Point chunkLocation)
    {
        if (_camera.Position.Z > 40)
        {
            // point-light performance isn't that great when rendering many chunks. lets just
            // disable point-lights when you zoom out too far. this shouldn't happen in regular play.
            return buffer[..0];
        }
        var minX = chunkLocation.X * LevelProvider.ChunkSize;
        var minY = chunkLocation.Y * LevelProvider.ChunkSize;
        var maxX = minX + LevelProvider.ChunkSize;
        var maxY = minY + LevelProvider.ChunkSize;

        // TODO: Performance is terrible. Lights should be in a quadtree for optimization.
        var index = 0;
        foreach (var light in _levelProvider.Map!.Lights)
        {
            if (!IsInRadius(minX, maxX, light.Radius, light.X) || !IsInRadius(minY, maxY, light.Radius, light.Y))
            {
                continue;
            }

            var point = new Vector3(light.X, light.Y, light.Z);
            var color = new Color(light.ARGB.R, light.ARGB.G, light.ARGB.B, light.ARGB.A);

            buffer[index] = new Light(point, color, light.Radius, light.Intensity / 256f);

            if (++index == buffer.Length)
            {
                return buffer;
            }
        }

        return buffer[..index];
    }

    private static bool IsInRadius(float min, float max, float radius, float value) => min - radius <= value && max + radius >= value;
}