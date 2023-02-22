using Microsoft.Xna.Framework.Graphics;
using OpenGta2.GameData.Style;

namespace OpenGta2.Client.Levels;

public class StyleTextureSet
{
    private StyleTextureSet(Texture2D tilesTexture)
    {
        TilesTexture = tilesTexture;
    }

    public Texture2D TilesTexture { get; }

    public static StyleTextureSet Create(Style style, GraphicsDevice graphicsDevice)
    {
        return new StyleTextureSet(CreateTilesTexture(style, graphicsDevice));
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