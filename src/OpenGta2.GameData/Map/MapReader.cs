using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using OpenGta2.GameData.Riff;

namespace OpenGta2.GameData.Map
{
    public class MapReader
    {
        private const int SupportedVersion = 500;

        private readonly RiffReader _riffReader;

        public MapReader(RiffReader riffReader)
        {
            if (riffReader.Type != "GBMP" || riffReader.Version != SupportedVersion)
            {
                ThrowHelper.ThrowInvalidFileFormat();
            }

            _riffReader = riffReader;
        }

        public Map Read()
        {
            // We're ignoring UMAP, CMAP PSXM chunks

            CompressedMap map;
            MapObject[] objects;
            MapZone[] zones;
            TileAnimation[] animations;
            MapLight[] lights;

            using (var compressedMapChunk = _riffReader.GetRequiredChunk("DMAP"))
            {
                map = ParseMap(compressedMapChunk);
            }

            using (var objectsChunk = _riffReader.GetChunk("MOBJ"))
            {
                objects = ParseArray<MapObject>(objectsChunk);
            }

            using (var zonesChunk = _riffReader.GetChunk("ZONE"))
            {
                zones = ParseMapZones(zonesChunk);
            }

            using (var animationsChunk = _riffReader.GetChunk("ANIM"))
            {
                animations = ParseAnimations(animationsChunk);
            }

            using (var junctionsChunk = _riffReader.GetChunk("RGEN"))
            {
                // TODO
            }

            using (var lightsChunk = _riffReader.GetChunk("LGHT"))
            {
                lights = ParseArray<MapLight>(lightsChunk);
            }

            return new Map(map, objects, zones, animations, lights);
        }

        private const int MapWidth = 256;
        private const int MapHeight = 256;

        private static TileAnimation[] ParseAnimations(RiffChunk? chunk)
        {
            if (chunk == null)
                return Array.Empty<TileAnimation>();
            
            var stream = chunk.Stream;
            
            Span<byte> tilesBuffer = stackalloc byte[512];

            var result = new List<TileAnimation>();
            while (stream.Position < stream.Length)
            {
                stream.ReadExact(out TileAnimationHeader header);
                
                var tilesBytes = tilesBuffer[..(header.AnimLength * 2)];
                stream.ReadExact(tilesBytes);
                var tiles = MemoryMarshal.Cast<byte, ushort>(tilesBytes);

                result.Add(new TileAnimation
                {
                    Base = header.Base,
                    FrameRate = header.FrameRate,
                    Repeat = header.Repeat,
                    Tiles = tiles.ToArray()
                });
            }

            return result.ToArray();
        }

        private static MapZone[] ParseMapZones(RiffChunk? chunk)
        {
            if (chunk == null)
                return Array.Empty<MapZone>();

            var stream = chunk.Stream;
            
            Span<byte> nameBuffer = stackalloc byte[256];
            
            var result = new List<MapZone>();
            while (stream.Position < stream.Length)
            {
                stream.ReadExact(out MapZoneHeader header);
                
                var name = nameBuffer[..header.NameLength];
                stream.ReadExact(name);

                result.Add(new MapZone
                {
                    X = header.X,
                    Y = header.Y,
                    Width = header.Width,
                    Height = header.Height,
                    Type = header.Type,
                    Name = Encoding.ASCII.GetString(name)
                });
            }

            return result.ToArray();
        }

        private static T[] ParseArray<T>(RiffChunk? chunk) where T : struct
        {
            if (chunk == null)
                return Array.Empty<T>();
            
            var count = chunk.Stream.Length / Marshal.SizeOf<T>();

            var result = new T[count];
            chunk.Stream.ReadExact(result.AsSpan());

            return result;
        }

        private static CompressedMap ParseMap(RiffChunk chunk)
        {
            var stream = chunk.Stream;

            // the map data consists of the following structs (along with defined structs):
            // struct compressed_map
            // {
            //     UInt32 base [256][256];
            //     UInt32 column_words;
            //     UInt32 column[variable size – column_words]; // contains col_info structs
            //     UInt32 num_blocks;
            //     block_info block[variable size – num_blocks];
            // }

            // struct col_info
            // {
            //     UInt8 height;
            //     UInt8 offset;
            //     UInt16 pad;
            //     UInt32 blockd[variable size – height - offset];
            // }

            // read base

            var @base = new uint[MapHeight, MapWidth];
            var baseSpan = MemoryMarshal.CreateSpan(ref Unsafe.As<byte, uint>(ref MemoryMarshal.GetArrayDataReference(@base)), @base.Length);
            stream.ReadExact(baseSpan);
            
            // read columns
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
            var blocks = new BlockInfo[stream.ReadExactDoubleWord()];
            stream.ReadExact(blocks.AsSpan());

            return new CompressedMap(@base, columns, blocks);
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct TileAnimationHeader
        {
            [FieldOffset(0)] public readonly ushort Base;
            [FieldOffset(2)] public readonly byte FrameRate;
            [FieldOffset(3)] public readonly byte Repeat;
            [FieldOffset(4)] public readonly byte AnimLength;
            [FieldOffset(5)] private readonly byte _unused;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct MapZoneHeader
        {
            [FieldOffset(0)] public readonly ZoneType Type;
            [FieldOffset(1)] public readonly byte X;
            [FieldOffset(2)] public readonly byte Y;
            [FieldOffset(3)] public readonly byte Width;
            [FieldOffset(4)] public readonly byte Height;
            [FieldOffset(5)] public readonly byte NameLength;
        }
    }
}
