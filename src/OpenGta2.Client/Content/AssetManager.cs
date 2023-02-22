using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OpenGta2.Client.Rendering.Effects;
using SpriteEffect = OpenGta2.Client.Rendering.Effects.SpriteEffect;

namespace OpenGta2.Client.Content;

public sealed class AssetManager : IDisposable
{
    private BlockFaceEffect? _blockFaceEffect;
    private SpriteEffect? _spriteEffect;

    public void LoadContent(ContentManager contentManager)
    {
        _blockFaceEffect = new BlockFaceEffect(contentManager.Load<Effect>("BlockFaceEffect"));
        _spriteEffect = new SpriteEffect(contentManager.Load<Effect>("SpriteEffect"));
    }
    
    public BlockFaceEffect CreateBlockFaceEffect()
    {
        return (BlockFaceEffect)_blockFaceEffect!.Clone();
    }

    public SpriteEffect CreateSpriteEffect()
    {
        return (SpriteEffect)_spriteEffect!.Clone();
    }

    public void Dispose()
    {
        _blockFaceEffect?.Dispose();
    }
}