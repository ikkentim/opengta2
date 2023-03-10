using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OpenGta2.Client.Rendering.Effects;

namespace OpenGta2.Client.Content;

public sealed class AssetManager : IDisposable
{
    private BlockFaceEffect? _blockFaceEffect;
    private ScreenspaceSpriteEffect? _screenspaceSpriteEffect;
    private WorldSpriteEffect? _worldSpriteEffect;
    private SpriteFont? _debugFont;

    public void LoadContent(ContentManager contentManager)
    {
        _blockFaceEffect = new BlockFaceEffect(contentManager.Load<Effect>("BlockFaceEffect"));
        _screenspaceSpriteEffect = new ScreenspaceSpriteEffect(contentManager.Load<Effect>("ScreenspaceSpriteEffect"));
        _worldSpriteEffect = new WorldSpriteEffect(contentManager.Load<Effect>("ScreenspaceSpriteEffect"));
        _debugFont = contentManager.Load<SpriteFont>("DebugFont");
    }
    
    public BlockFaceEffect CreateBlockFaceEffect()
    {
        return (BlockFaceEffect)_blockFaceEffect!.Clone();
    }
    
    public ScreenspaceSpriteEffect CreateScreenspaceSpriteEffect()
    {
        return (ScreenspaceSpriteEffect)_screenspaceSpriteEffect!.Clone();
    }

    public WorldSpriteEffect CreateWorldSpriteEffect()
    {
        return (WorldSpriteEffect)_worldSpriteEffect!.Clone();
    }

    public SpriteFont GetDebugFont() => _debugFont!;

    public void Dispose()
    {
        _blockFaceEffect?.Dispose();
    }
}