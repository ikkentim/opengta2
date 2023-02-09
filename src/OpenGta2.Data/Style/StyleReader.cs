using System.Runtime.InteropServices;
using System.Xml.Linq;
using OpenGta2.Data.Riff;

namespace OpenGta2.Data.Style
{
    public class StyleReader
    {
        private readonly RiffReader _riffReader;

        public StyleReader(RiffReader riffReader)
        {
            if (riffReader.Type != "GBST" || riffReader.Version != 700)
            {
                throw new Exception("unsupported style file");
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
            var chunk = _riffReader.GetChunk("PALB");

            if (chunk == null)
            {
                throw new Exception("Missing palette base");
            }
            
            Span<byte> buffer = stackalloc byte[Marshal.SizeOf<PaletteBase>()];

            if (chunk.Stream.Length != buffer.Length)
            {
                throw new Exception("bad pallet base length");
            }

            chunk.Stream.ReadExact(buffer);
            return MemoryMarshal.Read<PaletteBase>(buffer);
        }

        private PaletteIndex ReadPaletteIndex()
        {
            var chunk = _riffReader.GetChunk("PALX");

            if (chunk == null)
            {
                throw new Exception("Missing palette index");
            }

            if (chunk.Stream.Length != PaletteIndex.PhysPaletteLength * 2)
            {
                throw new InvalidOperationException("bad pallet index size");
            }

            var physPalette = new ushort[PaletteIndex.PhysPaletteLength];
            
            chunk.Stream.ReadExact(physPalette.AsSpan());

            return new PaletteIndex(physPalette);
        }

        private PhysicalPalette ReadPhysicalPalettes()
        {
            var chunk = _riffReader.GetChunk("PPAL");

            if (chunk == null)
            {
                throw new Exception("Missing physical palettes");
            }

            var paletteSize = chunk.Stream.Length / Marshal.SizeOf<uint>();
            var palette = new BgraColor[paletteSize];
            chunk.Stream.ReadExact(palette.AsSpan());

            return new PhysicalPalette(palette);
        }
        
        private Tiles ReadTiles()
        {
            using var chunk = _riffReader.GetChunk("TILE");

            if (chunk == null)
            {
                throw new Exception("Missing tiles");
            }

            var data = new byte[chunk.Stream.Length];
            chunk.Stream.ReadExact(data);
            return new Tiles(data);
        }
    }
}
