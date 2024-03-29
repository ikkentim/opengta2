﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenGta2.Client.Assets.Effects;
using OpenGta2.Client.Diagnostics;
using OpenGta2.Client.Levels;
using OpenGta2.Client.Rendering;

namespace OpenGta2.Client.Components;

public class MapComponent : BaseDrawableComponent
{
    private readonly Camera _camera;
    private readonly LevelProvider _levelProvider;
    private readonly Controls _controls;
    private BlockFaceEffect? _blockFaceEffect;
    private bool _noon;

    public MapComponent(GtaGame game, Camera camera, LevelProvider levelProvider, Controls controls) : base(game)
    {
        _camera = camera;
        _levelProvider = levelProvider;
        _controls = controls;
    }

    protected override void LoadContent()
    {
        _blockFaceEffect = Game.AssetManager.CreateBlockFaceEffect();
        _blockFaceEffect.Tiles = _levelProvider.Textures!.TilesTexture;
    }
    
    public override void Update(GameTime gameTime)
    {
        _levelProvider.Update(_camera);

        if (_controls.IsKeyDown(Keys.OemTilde))
        {
            _noon = !_noon;
            _blockFaceEffect!.AmbientLevel = _noon ? 1 : 0.3f;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        Span<Light> lights = stackalloc Light[BlockFaceEffect.MaxLights];

        _blockFaceEffect!.View = _camera.ViewMatrix;
        _blockFaceEffect.Projection = _camera.Projection;

        PerformanceCounters.Drawing.StartMeasurement("DrawOpaque");
        foreach (var chunk in _levelProvider.GetRenderableChunks())
        {
            if (chunk.OpaquePrimitiveCount == 0) continue;
            
            ApplyChunk(chunk);
            
            _blockFaceEffect.SetLights(CollectLights(lights, chunk.ChunkLocation));
            _blockFaceEffect.CurrentTechnique.Passes["Opaque"].Apply();
            Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, chunk.OpaquePrimitiveCount);
        }

        PerformanceCounters.Drawing.StopMeasurement();
        
        PerformanceCounters.Drawing.StartMeasurement("DrawFlat");

        foreach (var chunk in _levelProvider.GetRenderableChunks())
        {
            if (chunk.FlatPrimitiveCount == 0) continue;

            ApplyChunk(chunk);
            
            _blockFaceEffect.SetLights(CollectLights(lights, chunk.ChunkLocation));
            _blockFaceEffect.CurrentTechnique.Passes["Flat"].Apply();
            Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, chunk.FlatIndexOffset, chunk.FlatPrimitiveCount);
        }
        
        PerformanceCounters.Drawing.StopMeasurement();


        base.Draw(gameTime);
    }

    private void ApplyChunk(RenderableMapChunk chunk)
    {
        Game.GraphicsDevice.Indices = chunk.Indices;
        Game.GraphicsDevice.SetVertexBuffer(chunk.Vertices);
        _blockFaceEffect!.World = chunk.Translation;
    }
    

    private Span<Light> CollectLights(Span<Light> buffer, Point chunkLocation)
    {
        if (_noon)
        {
            return buffer[..0];
        }

        PerformanceCounters.Drawing.StartMeasurement("CollectLights");

        // point-light performance isn't that great when rendering many chunks. lets just
        // disable point-lights when you zoom out too far. this shouldn't happen in regular play.
        var result = _camera.Position.Z > 40 ? buffer[..0] : _levelProvider.CollectLights(buffer, chunkLocation);

        PerformanceCounters.Drawing.StopMeasurement();

        return result;
    }
}