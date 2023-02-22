using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenGta2.GameData.Style;
using SharpDX.Direct3D9;

namespace OpenGta2.Client.Levels;

public class StyleTextureSet
{
    private StyleTextureSet(Texture2D tilesTexture, Texture2D spritesTexture)
    {
        TilesTexture = tilesTexture;
        SpritesTexture = spritesTexture;
    }
    
    public Texture2D TilesTexture { get; }
    public Texture2D SpritesTexture { get; }

    public static StyleTextureSet Create(Style style, GraphicsDevice graphicsDevice)
    {
        return new StyleTextureSet(CreateTilesTexture(style, graphicsDevice), CreateSpritesTexture(style, graphicsDevice));
    }

    private static Texture2D CreateSpritesTexture(Style style, GraphicsDevice graphicsDevice)
    {
        var spritePages = style.SpriteGraphics.Length;

        // TODO: remove magic numbers
        var result = new Texture2D(graphicsDevice, 256, 256, false, SurfaceFormat.Color, spritePages);

        var pageData = new uint[256 * 256];
        var pageNumber = 0;
        foreach (var spritePage in style.SpriteGraphics)
        {
            foreach (var sprite in style.SpriteEntries.Select((sprite, index) => (index, sprite)).Where(s => s.sprite.PageNumber == pageNumber))
            {
                var spritePosition = new Point((int)sprite.sprite.PageX, (int)sprite.sprite.PageY);
                var size = new Point(sprite.sprite.Width, sprite.sprite.Height);

                var physicalPaletteNumber = style.PaletteIndex.PhysPalette[sprite.index + style.PaletteBase.SpriteOffset];
                var palette = style.PhysicsalPalette.GetPalette(physicalPaletteNumber);

                for (var y = 0; y < size.Y; y++)
                {
                    for (var x = 0; x < size.X; x++)
                    {
                        var point = new Point(x, y);

                        var pagePosition = spritePosition + point;

                        var pixelIndex = pagePosition.Y * 256 + pagePosition.X;
                        var colorEntry = spritePage.Data[pixelIndex];

                        pageData[pixelIndex] = colorEntry == 0
                            ? 0
                            : palette.GetColor(colorEntry)
                                .Argb;
                    }
                }
            }

            result.SetData(0, pageNumber, null, pageData, 0, pageData.Length);
            pageNumber++;
        }

        return result;
    }
    private static Texture2D CreateTilesTexture(Style style, GraphicsDevice graphicsDevice)
    {
        var tileCount = style.Tiles.Count;

        var result = new Texture2D(graphicsDevice, Tiles.TileWidth, Tiles.TileHeight, false, SurfaceFormat.Color,
            tileCount);

        // style can contain up to 992 tiles, each tile is 64x64 pixels.
        var tileData = new uint[Tiles.TileWidth * Tiles.TileHeight];

        for (ushort tileNumber = 0; tileNumber < tileCount; tileNumber++)
        {
            // don't need to add a base for virtual palette number - base for tiles is always 0.
            var physicalPaletteNumber = style.PaletteIndex.PhysPalette[tileNumber];
            var palette = style.PhysicsalPalette.GetPalette(physicalPaletteNumber);

            for (var y = 0; y < Tiles.TileHeight; y++)
            {
                var tileSlice = style.Tiles.GetTileSlice(tileNumber, y);
                for (var x = 0; x < Tiles.TileWidth; x++)
                {
                    var colorEntry = tileSlice[x]; // 0 is always transparent
                    tileData[y * Tiles.TileWidth + x] = colorEntry == 0
                        ? 0
                        : palette.GetColor(colorEntry)
                            .Argb;
                }
            }

            result.SetData(0, tileNumber, null, tileData, 0, tileData.Length);
        }

        return result;
    }
}