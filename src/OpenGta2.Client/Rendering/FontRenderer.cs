using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenGta2.Client.Assets;
using OpenGta2.Client.Assets.Effects;
using OpenGta2.Client.Levels;
using OpenGta2.GameData.Style;

namespace OpenGta2.Client.Rendering;

public class FontRenderer
{
    private readonly LevelProvider _levelProvider;
    private readonly ScreenspaceSpriteEffect _screenspaceSpriteEffect;

    public FontRenderer(AssetManager assetManager, LevelProvider levelProvider)
    {
        _levelProvider = levelProvider;
        _screenspaceSpriteEffect = assetManager.CreateScreenspaceSpriteEffect();
    }

    public void Draw(GraphicsDevice graphicsDevice, Vector2 point, int index, string text, int remap = -1)
    {

        var fontOffset = _levelProvider.Style.FontBase.GetFontOffset(index);
        
        var spaceSize = _levelProvider.Textures.GetSpriteTexture(SpriteKind.Font, (ushort)(fontOffset + ('.' - '!')), remap).Width;

        foreach(var c in text)
        {
            var ch = c;
            if (ch == ' ')
            {
                // TODO: Don't know how space is handled yet.
                point.X += spaceSize;
                continue;
            }

            var charNum = ch - '!';

            if (charNum < 0)
            {
                charNum = '?' - '!';
            }
            if (ch > '~')
            {
                // TODO: Haven't figured out the rest of the charset yet
                charNum = '?' - '!';
            }

            _screenspaceSpriteEffect.Texture = _levelProvider.Textures.GetSpriteTexture(SpriteKind.Font, (ushort)(fontOffset + charNum));
            _screenspaceSpriteEffect.CurrentTechnique.Passes[0].Apply();

            QuadRenderer.Render(graphicsDevice, point, point + new Vector2(_screenspaceSpriteEffect.Texture.Width, _screenspaceSpriteEffect.Texture.Height));

            point.X += _screenspaceSpriteEffect.Texture.Width;
        }
    }
}