using System.Text;
using OpenGta2.Data.Riff;

namespace OpenGta2.Data;

public class GtaStringReader
{
    private readonly RiffReader _reader;

    public GtaStringReader(RiffReader reader)
    {
        if (reader.Type.Substring(0, 3) != "GBL" || reader.Version != 0x64)
        {
            throw new Exception("unsupported string file");
        }
            
        _reader = reader;
    }

    public IDictionary<string, string> Read()
    {
        var stringBuilder = new StringBuilder();
        var keys = new Dictionary<string, uint>();
        var result = new Dictionary<string, string>();
        
        // read keys
        var keysChunk = _reader.GetChunk("TKEY") ?? throw new Exception("missing keys");
        while (keysChunk.Stream.Position < keysChunk.Stream.Length)
        {
            var dataOffset = keysChunk.Stream.ReadExactDoubleWord();
            var name = keysChunk.Stream.ReadExactString(8);
            keys[name] = dataOffset;
        }

        // read data
        var dataChunk = _reader.GetChunk("TDAT") ?? throw new Exception("missing data");
        foreach (var kv in keys)
        {
            dataChunk.Stream.Seek(kv.Value, SeekOrigin.Begin);
            result[kv.Key] = ReadString(dataChunk.Stream, stringBuilder);
        }
        
        return result;
    }

    private static string ReadString(Stream stream, StringBuilder stringBuilder)
    {
        stringBuilder.Clear();

        Span<byte> buffer = stackalloc byte[1];
        while (true)
        {
            var character = stream.ReadByte();
            var modifier = stream.ReadByte();

            if (character == 0 && modifier == 0 || character < 0 || modifier < 0)
            {
                break;
            }

            if (modifier != 0)
            {
                buffer[0] = (byte)modifier;
                stringBuilder.Append(Encoding.UTF8.GetString(buffer));
            }

            buffer[0] = (byte)character;
            stringBuilder.Append(Encoding.UTF8.GetString(buffer));
        }

        return stringBuilder.ToString();
    }
}