using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace OpenGta2.Client;

public class AssetManager
{
    private BlockFaceEffect? _blockFaceEffect;

    public void LoadContent(ContentManager contentManager)
    {
        _blockFaceEffect = new BlockFaceEffect(contentManager.Load<Effect>("BlockFaceEffect"));
    }

    public BlockFaceEffect CreateBlockFaceEffect() => (BlockFaceEffect)_blockFaceEffect!.Clone();
}