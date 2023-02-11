using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenGta2.Client;

public class BlockFaceEffect : Effect, IEffectMatrices
{
    private readonly EffectParameter _worldViewProjectionParam;
    private readonly EffectParameter _tilesParam;

    private DirtyFlags _dirtyFlags;
    
    private Matrix _projection;
    private Matrix _view;
    private Matrix _world;
    private Texture2D? _tiles;

    public BlockFaceEffect(Effect cloneSource) : base(cloneSource)
    {
        _worldViewProjectionParam = Parameters["WorldViewProjection"];
        _tilesParam = Parameters["Tiles"];
    }
    
    public Matrix Projection
    {
        get => _projection;
        set
        {
            _projection = value;
            _dirtyFlags |= DirtyFlags.WorldViewProjection;
        }
    }
    
    public Matrix View
    {
        get => _view;
        set
        {
            _view = value;
            _dirtyFlags |= DirtyFlags.WorldViewProjection;
        }
    }
    
    public Matrix World
    {
        get => _world;
        set
        {
            _world = value;
            _dirtyFlags |= DirtyFlags.WorldViewProjection;
        }
    }

    public Texture2D Tiles
    {
        get => _tiles;
        set
        {
            _tiles = value;
            _dirtyFlags |= DirtyFlags.Tiles;
        }
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
        Tiles = 2
    }
}