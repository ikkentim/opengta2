using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OpenGta2.Client.Rendering;

namespace OpenGta2.Client.Content;

public sealed class AssetManager : IDisposable
{
    private BlockFaceEffect? _blockFaceEffect;

    public void LoadContent(ContentManager contentManager)
    {
        _blockFaceEffect = new BlockFaceEffect(contentManager.Load<Effect>("BlockFaceEffect"));
    }

    public BlockFaceEffect CreateBlockFaceEffect()
    {
        return (BlockFaceEffect)_blockFaceEffect!.Clone();
    }

    public void Dispose()
    {
        _blockFaceEffect?.Dispose();
    }
}