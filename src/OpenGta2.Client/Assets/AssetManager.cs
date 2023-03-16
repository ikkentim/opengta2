using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OpenGta2.Client.Assets.Effects;

namespace OpenGta2.Client.Assets;

public sealed class AssetManager : IDisposable
{
    private BlockFaceEffect? _blockFaceEffect;
    private ScreenspaceSpriteEffect? _screenspaceSpriteEffect;
    private WorldSpriteEffect? _worldSpriteEffect;
    private SpriteFont? _debugFont;
    private DebugLineEffect? _debugLineEffect;

    public void LoadContent(ContentManager contentManager)
    {
        _blockFaceEffect = new BlockFaceEffect(contentManager.Load<Effect>("Effects/BlockFaceEffect"));
        _screenspaceSpriteEffect = new ScreenspaceSpriteEffect(contentManager.Load<Effect>("Effects/ScreenspaceSpriteEffect"));
        _worldSpriteEffect = new WorldSpriteEffect(contentManager.Load<Effect>("Effects/ScreenspaceSpriteEffect"));
        _debugLineEffect = new DebugLineEffect(contentManager.Load<Effect>("Effects/DebugLineEffect"));
        _debugFont = contentManager.Load<SpriteFont>("Fonts/DebugFont");
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

    public DebugLineEffect CreateDebugLineEffect()
    {
        return (DebugLineEffect)_debugLineEffect!.Clone();
    }

    public SpriteFont GetDebugFont() => _debugFont!;

    public void Dispose()
    {
        _blockFaceEffect?.Dispose();
    }
}