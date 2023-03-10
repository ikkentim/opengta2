using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenGta2.GameData.Style;

namespace OpenGta2.Client.Levels;

public class StyleTextureSet
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Style _style;
    private readonly uint[] _buffer = new uint[256 * 256];
    private readonly Dictionary<(SpriteKind kind, ushort number, int remap), Texture2D> _sprites = new(); // TODO: Disposing

    private StyleTextureSet(Texture2D tilesTexture, GraphicsDevice graphicsDevice, Style style)
    {
        _graphicsDevice = graphicsDevice;
        _style = style;
        TilesTexture = tilesTexture;
    }
    
    public Texture2D TilesTexture { get; }

    public static StyleTextureSet Create(Style style, GraphicsDevice graphicsDevice)
    {
        return new StyleTextureSet(CreateTilesTexture(style, graphicsDevice), graphicsDevice, style);
    }


    public Texture2D GetSpriteTexture(SpriteKind kind, ushort number, int remap = -1)
    {
        if (_sprites.TryGetValue((kind, number, remap), out var texture))
        {
            return texture;
        }

        texture = CreateSpriteTexture(kind, number, remap);
        _sprites[(kind, number, remap)] = texture;

        return texture;
    }

    private Texture2D CreateSpriteTexture(SpriteKind kind, ushort number, int remap)
    {
        var sprite = GetSprite(kind, number);

        var virtualPaletteNumber = sprite.Number + _style.PaletteBase.SpriteOffset;

        if (remap >= 0)
        {
            var remaps = _style.PaletteBase.GetRemap(kind);

            if (remaps <= remap)
            {
                throw new ArgumentOutOfRangeException(nameof(remap));
            }

            var remapPalette = _style.PaletteBase.GetRemapOffset(kind);
            virtualPaletteNumber = remapPalette + remap;
        }

        var physicalPaletteNumber = _style.PaletteIndex.PhysPalette[virtualPaletteNumber];
        var palette = _style.PhysicsalPalette.GetPalette(physicalPaletteNumber);

        for (byte y = 0; y < sprite.Height; y++)
        {
            for (byte x = 0; x < sprite.Width; x++)
            {
                var colorEntry = sprite[y, x];

                _buffer[y * sprite.Width + x] = colorEntry == 0
                    ? 0
                    : palette.GetColor(colorEntry)
                        .Argb;
            }
        }

        var texture = new Texture2D(_graphicsDevice, sprite.Width, sprite.Height, false, SurfaceFormat.Color);
        texture.SetData(_buffer, 0, sprite.Width * sprite.Height);

        return texture;
    }

    private Sprite GetSprite(SpriteKind kind, ushort number)
    {
        var spriteBase = _style.SpriteBases.GetOffset(kind);
        var entry = _style.SpriteEntries[spriteBase + number];
        var page = _style.SpriteGraphics[entry.PageNumber];
        return page.GetSprite(entry, (ushort)(spriteBase + number));
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
                        // var colorEntry = spritePage.Data[pixelIndex];

                        // pageData[pixelIndex] = colorEntry == 0
                        //     ? 0
                        //     : palette.GetColor(colorEntry)
                        //         .Argb;
                    }
                }
            }

            result.SetData(0, pageNumber, null, pageData, 0, pageData.Length);
            
            Array.Clear(pageData);
            pageNumber++;
        }

        return result;
    }

    private static Texture2D CreateTilesTexture(Style style, GraphicsDevice graphicsDevice)
    {
        var tileCount = style.Tiles.TileCount;

        
        var result = new Texture2D(graphicsDevice, Tile.Width, Tile.Height, false, SurfaceFormat.Color,
            tileCount);

        // style can contain up to 992 tiles, each tile is 64x64 pixels.
        var tileData = new uint[Tile.Width * Tile.Height];

        for (ushort tileNumber = 0; tileNumber < tileCount; tileNumber++)
        {
            // don't need to add a base for virtual palette number - base for tiles is always 0.
            var physicalPaletteNumber = style.PaletteIndex.PhysPalette[tileNumber];
            var palette = style.PhysicsalPalette.GetPalette(physicalPaletteNumber);

            var tile = style.Tiles.GetTile(tileNumber);

            for (byte y = 0; y < Tile.Height; y++)
            {
                for (byte x = 0; x < Tile.Width; x++)
                {
                    
                    var colorEntry = tile[y, x]; // 0 is always transparent
                    tileData[y * Tile.Width + x] = colorEntry == 0
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