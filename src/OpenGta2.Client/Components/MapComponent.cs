using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenGta2.Client.Assets.Effects;
using OpenGta2.Client.Diagnostics;
using OpenGta2.Client.Levels;
using OpenGta2.Client.Rendering;
using OpenGta2.Client.Utilities;

namespace OpenGta2.Client.Components;

public class MapComponent : BaseDrawableComponent
{
    private readonly Camera _camera;
    private readonly LevelProvider _levelProvider;
    private readonly Controls _controls;
    private BlockFaceEffect? _blockFaceEffect;

    private const float DuskAmbientLevel = 0.3f;
    private const float NoonAmbientLevel = 1f;

    private readonly DebugSettings _debugSettings;
    private static readonly RasterizerState _wireframe = new() { FillMode = FillMode.WireFrame, CullMode = CullMode.None };

    public MapComponent(GtaGame game, Camera camera, LevelProvider levelProvider, Controls controls, DebugSettings debugSettings) : base(game)
    {
        _debugSettings = debugSettings;
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
        _levelProvider.UpdateAnimations(gameTime.GetDelta());
        ShowAnimationSpeed();

        // the ~ key is a shortcut for the switch in the world debug screen
        if (_controls.IsKeyDown(Keys.OemTilde))
        {
            _debugSettings.Noon = !_debugSettings.Noon;
        }

        _blockFaceEffect!.AmbientLevel = _debugSettings.Noon ? NoonAmbientLevel : DuskAmbientLevel;
    }

    /// <summary>
    /// Debug: shows the speed of the tile animations (tuned in the world debug screen).
    /// </summary>
    private void ShowAnimationSpeed()
    {
        var animator = _levelProvider.TileAnimator;
        if (animator == null) return;

        DiagnosticValues.Set("TileAnimation", $"{animator.GameFramesPerSecond:F1} game frames/s ({animator.AnimationCount} animations)");
    }

    public override void Draw(GameTime gameTime)
    {
        Span<Light> lights = stackalloc Light[BlockFaceEffect.MaxLights];

        // always clamp: with wrapping, bilinear filtering at the edge of a tile blends in the opposite edge of the same tile.
        // Even clamped, bilinear filtering is flat for the last half texel at a tile edge, which bends diagonal lines (power lines)
        // a little where one tile ends and the next begins; point sampling is exact.
        Game.GraphicsDevice.SamplerStates[0] = (_debugSettings.PointSampling, _debugSettings.WrapSampling) switch
        {
            (true, true) => SamplerState.PointWrap,
            (true, false) => SamplerState.PointClamp,
            (false, true) => SamplerState.LinearWrap,
            _ => SamplerState.LinearClamp
        };

        _blockFaceEffect!.SetFlatCutoff(_debugSettings.FlatAlphaTest ? 0.5f : 0.05f);
        _blockFaceEffect.SetSharpTexels(_debugSettings.SharpTexels);

        const float scale = 63f / 64f;
        const float half = 0.5f / 64f;
        var normal = new Vector4(1, 1, 0, 0);

        // the edges of a block are the centres of the first and last texel: neighbouring tiles share their edge texel
        var sharedEdge = new Vector4(scale, scale, half, half);

        _blockFaceEffect.SetTileUv(
            _debugSettings.TileSpan == TileSpan.SharedEdgeAll ? sharedEdge : normal,
            _debugSettings.TileSpan is TileSpan.SharedEdgeAll or TileSpan.SharedEdgeFlat ? sharedEdge : normal);
        _blockFaceEffect.View = _camera.ViewMatrix;
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

        if (_debugSettings.ShowFlatOutlines)
        {
            DrawFlatOutlines();
        }

        base.Draw(gameTime);
    }

    /// <summary>
    /// Debug: draws the triangles of the flat faces as magenta lines on top of the map, to see where the geometry of transparent textures is.
    /// </summary>
    private void DrawFlatOutlines()
    {
        var rasterizer = Game.GraphicsDevice.RasterizerState;
        var depth = Game.GraphicsDevice.DepthStencilState;
        var blend = Game.GraphicsDevice.BlendState;
        Game.GraphicsDevice.RasterizerState = _wireframe;
        Game.GraphicsDevice.DepthStencilState = DepthStencilState.None;
        Game.GraphicsDevice.BlendState = BlendState.Opaque;

        foreach (var chunk in _levelProvider.GetRenderableChunks())
        {
            if (chunk.FlatPrimitiveCount == 0) continue;

            ApplyChunk(chunk);

            _blockFaceEffect!.CurrentTechnique.Passes["Outline"].Apply();
            Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, chunk.FlatIndexOffset, chunk.FlatPrimitiveCount);
        }

        Game.GraphicsDevice.RasterizerState = rasterizer;
        Game.GraphicsDevice.DepthStencilState = depth;
        Game.GraphicsDevice.BlendState = blend;
    }

    private void ApplyChunk(RenderableMapChunk chunk)
    {
        Game.GraphicsDevice.Indices = chunk.Indices;
        Game.GraphicsDevice.SetVertexBuffer(chunk.Vertices);
        _blockFaceEffect!.World = chunk.Translation;
    }
    

    private Span<Light> CollectLights(Span<Light> buffer, Point chunkLocation)
    {
        if (_debugSettings.Noon)
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