using System.Runtime.InteropServices;
using System.Text;
using OpenGta2.Data.Riff;

namespace OpenGta2.Data.Map
{
    public class MapReader
    {
        private readonly RiffReader _reader;

        public MapReader(RiffReader reader)
        {
            if (reader.Type != "GBMP" || reader.Version != 500)
            {
                throw new Exception("unsupported map file");
            }

            _reader = reader;
        }

        public Map Read()
        {
            // We're ignoring UMAP, CMAP PSXM chunks

            CompressedMap map;
            MapObject[] objects;
            MapZone[] zones;
            TileAnimation[] animations;

            using (var compressedMapChunk = _reader.GetChunk("DMAP") ?? throw new Exception("Missing map data"))
            {
                map = ParseMap(compressedMapChunk);
            }

            using (var objectsChunk = _reader.GetChunk("MOBJ"))
            {
                objects = ParseMapObjects(objectsChunk);
            }

            using (var zonesChunk = _reader.GetChunk("ZONE"))
            {
                zones = ParseMapZones(zonesChunk);
            }

            using (var animationsChunk = _reader.GetChunk("ANIM"))
            {
                animations = ParseAnimations(animationsChunk);
            }

            using (var junctionsChunk = _reader.GetChunk("RGEN"))
            {
                // TODO
            }

            using (var lightsChunk = _reader.GetChunk("LGHT"))
            {
                // TODO
            }

            return new Map(map, objects, zones, animations);
        }

        private const int MapWidth = 256;
        private const int MapHeight = 256;

        private static TileAnimation[] ParseAnimations(RiffChunk? chunk)
        {
            if (chunk == null)
                return Array.Empty<TileAnimation>();
            
            var stream = chunk.Stream;

            Span<byte> buffer = stackalloc byte[Marshal.SizeOf<TileAnimationHeader>()];
            Span<byte> tilesBuffer = stackalloc byte[512];

            var result = new List<TileAnimation>();
            while (stream.Position < stream.Length)
            {
                stream.ReadExact(buffer);

                var header = MemoryMarshal.Read<TileAnimationHeader>(buffer);
                var tilesBytes = tilesBuffer[..(header.AnimLength * 2)];
                stream.ReadExact(tilesBytes);
                var tiles = MemoryMarshal.Cast<byte, ushort>(tilesBytes);

                result.Add(new TileAnimation(header, tiles.ToArray()));
            }

            return result.ToArray();
        }

        private static MapZone[] ParseMapZones(RiffChunk? chunk)
        {
            if (chunk == null)
                return Array.Empty<MapZone>();

            var stream = chunk.Stream;

            Span<byte> buffer = stackalloc byte[Marshal.SizeOf<MapZoneHeader>()];
            Span<byte> nameBuffer = stackalloc byte[256];
            
            var result = new List<MapZone>();
            while (stream.Position < stream.Length)
            {
                stream.ReadExact(buffer);

                var header = MemoryMarshal.Read<MapZoneHeader>(buffer);
                var name = nameBuffer[..header.NameLength];
                stream.ReadExact(name);

                result.Add(new MapZone(header, Encoding.ASCII.GetString(name)));
            }

            return result.ToArray();
        }

        private static MapObject[] ParseMapObjects(RiffChunk? chunk)
        {
            if (chunk == null)
                return Array.Empty<MapObject>();

            var stream = chunk.Stream;

            Span<byte> buffer = stackalloc byte[Marshal.SizeOf<MapObject>()];

            var count = stream.Length / buffer.Length;

            var result = new MapObject[count];
            for (var i = 0; i < count; i++)
            {
                stream.ReadExact(buffer);

                result[i] = MemoryMarshal.Read<MapObject>(buffer);
            }

            return result;
        }

        private static CompressedMap ParseMap(RiffChunk chunk)
        {
            var stream = chunk.Stream;

            // struct compressed_map
            // {
            //     UInt32 base [256][256];
            //     UInt32 column_words;
            //     UInt32 column[variable size – column_words]; // contains col_info structs
            //     UInt32 num_blocks;
            //     block_info block[variable size – num_blocks];
            // }
            
            // base[256][256]
            var @base = new uint[MapWidth, MapHeight];

            for(var y = 0; y <MapHeight;y++)
            for (var x = 0; x < MapWidth; x++)
            {
                @base[x, y] = stream.ReadExactDoubleWord();
            }

            // read columns

            // struct col_info
            // {
            //     UInt8 height;
            //     UInt8 offset;
            //     UInt16 pad;
            //     UInt32 blockd[variable size – height - offset];
            // }

            var columnDoubleWords = stream.ReadExactDoubleWord();
            var columnsStart = stream.Position;
            var columnsEnd = columnsStart + columnDoubleWords * 4;
            var columns = new Dictionary<uint, ColumnInfo>();

            while (stream.Position < columnsEnd)
            {
                var columnOffset = stream.Position - columnsStart;

                var height = stream.ReadExactByte();
                var offset = stream.ReadExactByte();
                var blockIndices = new uint[height - offset];

                // skip padding
                stream.ReadExactWord();

                for (var i = 0; i < blockIndices.Length; i++)
                {
                    blockIndices[i] = stream.ReadExactDoubleWord();
                }

                columns[(uint)columnOffset / 4] = new ColumnInfo(height, offset, blockIndices);
            }

            // read blocks
            var blockCount = stream.ReadExactDoubleWord();
            var blocks = new BlockInfo[blockCount];
            
            Span<byte> blockBuffer = stackalloc byte[Marshal.SizeOf<BlockInfo>()];

            for (var i = 0; i < blockCount; i++)
            {
                stream.ReadExact(blockBuffer);
                blocks[i] = MemoryMarshal.Read<BlockInfo>(blockBuffer);
            }

            return new CompressedMap(@base, columns, NumBlocks: 0, blocks);
        }
    }
}
