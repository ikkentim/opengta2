using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenGta2.Client.Rendering.Effects;

public class BlockFaceEffect : Effect, IEffectMatrices
{
    public const int MaxLights = 16;

    private readonly EffectParameter _worldViewProjectionParam;
    private readonly EffectParameter _worldParam;
    private readonly EffectParameter _tilesParam;
    private readonly EffectParameter _lightPositionsParam;
    private readonly EffectParameter _lightColorsParam;
    private readonly EffectParameter _lightRadiiParam;
    private readonly EffectParameter _lightIntensitiesParam;
    private readonly EffectParameter _lightCountParam;
    private readonly EffectParameter _ambientLevelParam;
    private readonly EffectParameter _shadingLevelParam;

    private DirtyFlags _dirtyFlags;

    private Matrix _projection;
    private Matrix _view;
    private Matrix _world;
    private Texture2D? _tiles;
    private float _ambientLevel = 0.3f;
    private float _shadingLevel = 15;

    private readonly Vector3[] _lightPositions = new Vector3[MaxLights];
    private readonly Vector4[] _lightColors = new Vector4[MaxLights];
    private readonly float[] _lightRadii = new float[MaxLights];
    private readonly float[] _lightIntensities = new float[MaxLights];
    private int _lightCount;

    public BlockFaceEffect(Effect cloneSource) : base(cloneSource)
    {
        _worldViewProjectionParam = Parameters["WorldViewProjection"];
        _worldParam = Parameters["World"];
        _tilesParam = Parameters["Tiles"];
        _lightPositionsParam = Parameters["LightPositions"];
        _lightColorsParam = Parameters["LightColors"];
        _lightRadiiParam = Parameters["LightRadii"];
        _lightIntensitiesParam = Parameters["LightIntensities"];
        _lightCountParam = Parameters["LightCount"];
        _ambientLevelParam = Parameters["AmbientLevel"];
        _shadingLevelParam = Parameters["ShadingLevel"];
    }

    public Matrix Projection
    {
        get => _projection;
        set => Set(ref _projection, value, DirtyFlags.WorldViewProjection);
    }

    public Matrix View
    {
        get => _view;
        set => Set(ref _view, value, DirtyFlags.WorldViewProjection);
    }

    public Matrix World
    {
        get => _world;
        set => Set(ref _world, value, DirtyFlags.WorldViewProjection | DirtyFlags.World);
    }

    public Texture2D? Tiles
    {
        get => _tiles;
        set => Set(ref _tiles, value, DirtyFlags.Tiles);
    }

    /// <summary>
    /// Ambient light level. 0.0 is black, 1.0 is ‘normal’ GTA without light. 0.3 is 'dusk' on Industrial map.
    /// </summary>
    public float AmbientLevel
    {
        get => _ambientLevel;
        set => Set(ref _ambientLevel, value, DirtyFlags.AmbientLevel);
    }

    /// <summary>
    /// Similar to the AmbientLevel, this sets the shading 'contrast' for the level. Valid values are 0 – 31, with 15 being the 'normal' level.
    /// </summary>
    public float ShadingLevel
    {
        get => _shadingLevel;
        set => Set(ref _shadingLevel, value, DirtyFlags.ShadingLevel);
    }

    public void SetLights(Span<Light> lights)
    {
        var index = 0;
        foreach (var light in lights)
        {
            _lightPositions[index] = light.Position;
            _lightColors[index] = light.Color.ToVector4();
            _lightRadii[index] = light.Radius;
            _lightIntensities[index] = light.Intensity;
            _dirtyFlags |= DirtyFlags.Lights;

            index++;
            if (index == MaxLights)
                break;
        }

        _lightCount = index;
        _dirtyFlags |= DirtyFlags.LightCount;
    }

    private void Set<T>(ref T field, T value, DirtyFlags flag)
    {
        if (field?.Equals(value) ?? false)
            return;

        field = value;
        _dirtyFlags |= flag;
    }

    protected override void OnApply()
    {
        if ((_dirtyFlags & DirtyFlags.WorldViewProjection) != 0)
            _worldViewProjectionParam.SetValue(World * View * Projection);

        if ((_dirtyFlags & DirtyFlags.World) != 0)
            _worldParam.SetValue(World);

        if ((_dirtyFlags & DirtyFlags.Tiles) != 0)
            _tilesParam.SetValue(_tiles);

        if ((_dirtyFlags & DirtyFlags.Lights) != 0)
        {
            _lightPositionsParam.SetValue(_lightPositions);
            _lightColorsParam.SetValue(_lightColors);
            _lightRadiiParam.SetValue(_lightRadii);
            _lightIntensitiesParam.SetValue(_lightIntensities);
        }

        if ((_dirtyFlags & DirtyFlags.LightCount) != 0)
            _lightCountParam.SetValue(_lightCount);

        if ((_dirtyFlags & DirtyFlags.AmbientLevel) != 0)
            _ambientLevelParam.SetValue(_ambientLevel);

        if ((_dirtyFlags & DirtyFlags.ShadingLevel) != 0)
            _shadingLevelParam.SetValue(_shadingLevel);

        _dirtyFlags = DirtyFlags.None;

        base.OnApply();
    }

    public override Effect Clone()
    {
        return new BlockFaceEffect(this);
    }

    [Flags]
    private enum DirtyFlags
    {
        None = 0,
        WorldViewProjection = 1,
        Tiles = 2,
        Lights = 4,
        LightCount = 8,
        World = 16,
        AmbientLevel = 32,
        ShadingLevel = 64
    }
}