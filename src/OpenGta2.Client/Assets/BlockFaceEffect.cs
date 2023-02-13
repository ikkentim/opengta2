using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenGta2.Client.Effects;

public class BlockFaceEffect : Effect, IEffectMatrices
{
    private readonly EffectParameter _worldViewProjectionParam;
    private readonly EffectParameter _tilesParam;
    private readonly EffectParameter _flatParam;

    private DirtyFlags _dirtyFlags;
    
    private bool _flat;
    private Matrix _projection;
    private Matrix _view;
    private Matrix _world;
    private Texture2D? _tiles;

    public BlockFaceEffect(Effect cloneSource) : base(cloneSource)
    {
        _worldViewProjectionParam = Parameters["WorldViewProjection"];
        _tilesParam = Parameters["Tiles"];
        _flatParam = Parameters["Flat"];
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
        set => Set(ref _world, value, DirtyFlags.WorldViewProjection);
    }

    public Texture2D? Tiles
    {
        get => _tiles;
        set => Set(ref _tiles, value, DirtyFlags.Tiles);
    }
    
    public bool Flat
    {
        get => _flat;
        set => Set(ref _flat, value, DirtyFlags.Flat);
    }

    private void Set<T>(ref T field, T value, DirtyFlags flag)
    {
        if (field?.Equals(value) ?? false)
        {
            return;
        }

        field = value;
        _dirtyFlags |= flag;
    }

    protected override void OnApply()
    {
        if ((_dirtyFlags & DirtyFlags.WorldViewProjection) != 0)
        {
            _worldViewProjectionParam.SetValue(World * View * Projection);
        }

        if ((_dirtyFlags & DirtyFlags.Tiles) != 0)
        {
            _tilesParam.SetValue(_tiles);
        }

        if ((_dirtyFlags & DirtyFlags.Flat) != 0)
        {
            _flatParam.SetValue(_flat);
        }

        _dirtyFlags = DirtyFlags.None;

        base.OnApply();
    }

    public override Effect Clone() => new BlockFaceEffect(this);

    [Flags]
    private enum DirtyFlags
    {
        None = 0,
        WorldViewProjection = 1,
        Tiles = 2,
        Flat = 4,
    }
}