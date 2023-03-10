using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenGta2.Client.Content;
using OpenGta2.Client.Diagnostics;
using OpenGta2.Client.Levels;
using OpenGta2.GameData.Style;
using SpriteEffect = OpenGta2.Client.Rendering.Effects.SpriteEffect;

namespace OpenGta2.Client.Rendering;

public class FontRenderer
{
    private readonly LevelProvider _levelProvider;
    private readonly SpriteEffect _spriteEffect;

    public FontRenderer(AssetManager assetManager, LevelProvider levelProvider)
    {
        _levelProvider = levelProvider;
        _spriteEffect = assetManager.CreateSpriteEffect();
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

            _spriteEffect.Texture = _levelProvider.Textures.GetSpriteTexture(SpriteKind.Font, (ushort)(fontOffset + charNum));
            _spriteEffect.CurrentTechnique.Passes[0].Apply();

            QuadRenderer.Render(graphicsDevice, point, point + new Vector2(_spriteEffect.Texture.Width, _spriteEffect.Texture.Height));

            point.X += _spriteEffect.Texture.Width;
        }
    }
}