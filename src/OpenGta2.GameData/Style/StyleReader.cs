using System.Runtime.InteropServices;
using OpenGta2.GameData.Riff;

namespace OpenGta2.GameData.Style
{
    public class StyleReader
    {
        private const int SupportedVersion = 700;

        private readonly RiffReader _riffReader;

        public StyleReader(RiffReader riffReader)
        {
            if (riffReader.Type != "GBST" || riffReader.Version != SupportedVersion)
            {
                ThrowHelper.ThrowInvalidFileFormat();
            }

            _riffReader = riffReader;
        }

        public Style Read()
        {
            var paletteBase = ReadPaletteBase();
            var paletteIndex = ReadPaletteIndex();
            var physPalette = ReadPhysicalPalettes();
            var tiles = ReadTiles();

            var spriteGraphics = ReadSpriteGraphics();
            var spriteBases = ReadSpriteBases();
            var spriteIndex = ReadSpriteIndex();

            var fontBase = ReadFontBase();

            return new Style(paletteBase, paletteIndex, physPalette, tiles, spriteGraphics, spriteBases, spriteIndex, fontBase);
        }

        private PaletteBase ReadPaletteBase()
        {
            using var chunk = _riffReader.GetRequiredChunk("PALB", Marshal.SizeOf<PaletteBase>());
            chunk.Stream.ReadExact(out PaletteBase result);
            return result;
        }

        private PaletteIndex ReadPaletteIndex()
        {
            using var chunk = _riffReader.GetRequiredChunk("PALX", PaletteIndex.PhysPaletteLength * 2);

            var physPalette = new ushort[PaletteIndex.PhysPaletteLength];
            chunk.Stream.ReadExact(physPalette.AsSpan());

            return new PaletteIndex(physPalette);
        }

        private PhysicalPalette ReadPhysicalPalettes()
        {
            var chunk = _riffReader.GetRequiredChunk("PPAL");

            var paletteSize = chunk.Stream.Length / Marshal.SizeOf<uint>();
            var palette = new BgraColor[paletteSize];
            chunk.Stream.ReadExact(palette.AsSpan());

            return new PhysicalPalette(palette);
        }

        private FontBase ReadFontBase()
        {
            using var chunk = _riffReader.GetRequiredChunk("FONB");

            var count = chunk.Stream.ReadExactWord();

            var data = new ushort[count];
            chunk.Stream.ReadExact(data.AsSpan());

            return new FontBase(data);
        }
        private Tiles ReadTiles()
        {
            using var chunk = _riffReader.GetRequiredChunk("TILE");

            var data = new byte[chunk.Stream.Length];
            chunk.Stream.ReadExact(data);

            return new Tiles(data);
        }

        private SpritePage[] ReadSpriteGraphics()
        {
            using var chunk = _riffReader.GetRequiredChunk("SPRG");

            var lengthCheck = chunk.Stream.Length % (256 * 256);

            if (lengthCheck != 0)
            {
                throw new InvalidOperationException($"Unexpected chunk length of {chunk.Stream.Length}. Is offset by {lengthCheck}");
            }

            const int PageSize = 256 * 256;

            var pageCount = chunk.Stream.Length / PageSize;
            
            var pages = new SpritePage[pageCount];

            for (var page = 0; page < pageCount; page++)
            {
                var data = new byte[PageSize];
                chunk.Stream.ReadExact(data.AsSpan());
                pages[page] = new SpritePage(data);
            }
            
            return pages;
        }

        private SpriteEntry[] ReadSpriteIndex()
        {
            using var chunk = _riffReader.GetRequiredChunk("SPRX");

            var spriteCount = chunk.Stream.Length / Marshal.SizeOf<SpriteEntry>();

            var result = new SpriteEntry[spriteCount];
            chunk.Stream.ReadExact(result.AsSpan());

            return result;
        }

        private SpriteBase ReadSpriteBases()
        {
            using var chunk = _riffReader.GetRequiredChunk("SPRB", Marshal.SizeOf<SpriteBase>());
            chunk.Stream.ReadExact(out SpriteBase result);
            return result;
        }
    }
}
