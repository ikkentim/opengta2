using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenGta2.Client.Content;
using OpenGta2.Client.Diagnostics;
using OpenGta2.Client.Levels;
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
        _spriteEffect.Texture = levelProvider.Textures.SpritesTexture;
    }

    public void Draw(GraphicsDevice graphicsDevice, Vector2 point, int index, string text)
    {
        _spriteEffect!.Texture = _levelProvider.Textures.SpritesTexture;
        _spriteEffect.CurrentTechnique.Passes[0].Apply();

        var fontOffset = _levelProvider.Style.FontBase.GetFontOffset(index);

        var spriteFontOffset = _levelProvider.Style.SpriteBases.FontOffset;
        

        foreach(var c in text)
        {
            var ch = c;
            if (ch == ' ')
            {
                // TODO: Don't know how space is handled yet.
                point.X += _levelProvider.Style.SpriteEntries[spriteFontOffset + fontOffset + ('.' - '!')].Width;
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
            var entry = _levelProvider.Style.SpriteEntries[spriteFontOffset + fontOffset + charNum];
            
            QuadRenderer.Render(graphicsDevice, entry.PageNumber, point, point + new Vector2(entry.Width, entry.Height),
                new Vector2(entry.PageX, entry.PageY) / 256, new Vector2(entry.PageX + entry.Width, entry.PageY + entry.Height) / 256);

            point.X += entry.Width;
        }
    }
}