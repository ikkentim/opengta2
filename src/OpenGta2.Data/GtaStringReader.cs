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
        var result = new Dictionary<string, string>();
        
        // read keys
        var keyss = _reader.GetChunk("TKEY") ?? throw new Exception("missing keys");

        var keysDict = new Dictionary<string, uint>();
        while (keyss.Stream.Position < keyss.Stream.Length)
        {
            var dataOffset = keyss.Stream.ReadExactDoubleWord();
            var name = keyss.Stream.ReadExactString(8);
            keysDict[name] = dataOffset;
        }

        // read data
        var dat = _reader.GetChunk("TDAT") ?? throw new Exception("missing data");
        foreach (var kv in keysDict)
        {
            dat.Stream.Seek(kv.Value, SeekOrigin.Begin);
            ReadString(dat.Stream, stringBuilder);
            
            result[kv.Key] = stringBuilder.ToString();

            stringBuilder.Clear();
        }
        
        return result;
    }

    private static void ReadString(Stream stream, StringBuilder stringBuilder)
    {
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
    }
}