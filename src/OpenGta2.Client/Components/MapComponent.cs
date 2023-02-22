using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenGta2.Client.Content;
using OpenGta2.Client.Levels;
using OpenGta2.Client.Rendering;
using OpenGta2.Client.Rendering.Effects;

namespace OpenGta2.Client.Components;

public class MapComponent : DrawableGameComponent
{
    private readonly GtaGame _game;
    private readonly Camera _camera;
    private readonly LevelProvider _levelProvider;
    private BlockFaceEffect? _blockFaceEffect;

    public MapComponent(GtaGame game, Camera camera) : base(game)
    {
        _game = game;
        _camera = camera;

        _levelProvider = game.Services.GetService<LevelProvider>();
    }

    protected override void LoadContent()
    {
        _blockFaceEffect = _game.AssetManager.CreateBlockFaceEffect();
        _blockFaceEffect.Tiles = _levelProvider.Textures!.TilesTexture;
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

        foreach (var chunk in _levelProvider.GetRenderableChunks())
        {
            if (chunk.OpaquePrimitiveCount == 0) continue;
            
            ApplyChunk(chunk);
            
            _blockFaceEffect.SetLights(CollectLights(lights, chunk.ChunkLocation));
            _blockFaceEffect.CurrentTechnique.Passes["Opaque"].Apply();
            _game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, chunk.OpaquePrimitiveCount);
        }
        
        foreach (var chunk in _levelProvider.GetRenderableChunks())
        {
            if (chunk.FlatPrimitiveCount == 0) continue;

            ApplyChunk(chunk);
            
            _blockFaceEffect.SetLights(CollectLights(lights, chunk.ChunkLocation));
            _blockFaceEffect.CurrentTechnique.Passes["Flat"].Apply();
            _game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, chunk.FlatIndexOffset, chunk.FlatPrimitiveCount);
        }


        base.Draw(gameTime);
    }

    private void ApplyChunk(RenderableMapChunk chunk)
    {
        _game.GraphicsDevice.Indices = chunk.Indices;
        _game.GraphicsDevice.SetVertexBuffer(chunk.Vertices);
        _blockFaceEffect!.World = chunk.Translation;
    }
    

    private Span<Light> CollectLights(Span<Light> buffer, Point chunkLocation)
    {
        // point-light performance isn't that great when rendering many chunks. lets just
        // disable point-lights when you zoom out too far. this shouldn't happen in regular play.
        return _camera.Position.Z > 40 ? buffer[..0] : _levelProvider.CollectLights(buffer, chunkLocation);
    }
}