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

            return new Style(paletteBase, paletteIndex, physPalette, tiles);
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
        
        private Tiles ReadTiles()
        {
            using var chunk = _riffReader.GetRequiredChunk("TILE");
            
            var data = new byte[chunk.Stream.Length];
            chunk.Stream.ReadExact(data);

            return new Tiles(data);
        }
    }
}
