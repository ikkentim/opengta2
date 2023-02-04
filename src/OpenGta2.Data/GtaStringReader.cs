using System.Text;

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
        // find chunks
        RiffChunk? keys = null;
        RiffChunk? data = null;

        foreach (var chunk in _reader.ReadChunks())
        {
            if (chunk.Name == "TKEY")
            {
                keys = chunk;
            }
            else if (chunk.Name == "TDAT")
            {
                data = chunk;
            }
        }

        if (keys == null || data == null)
        {
            throw new Exception("missing data");
        }
            
        // match keys and data
        var result = new Dictionary<string, string>();
        var keysStream = new MemoryStream(keys.Data);
        var stringBuilder = new StringBuilder();
        while (keysStream.Position < keysStream.Length)
        {
            var dataOffset = keysStream.ReadExactDoubleWord();
            var name = keysStream.ReadExactString(8);

            ReadString(dataOffset, data, stringBuilder);

            result[name] = stringBuilder.ToString();

            stringBuilder.Clear();
        }

        return result;
    }

    private static void ReadString(uint dataOffset, RiffChunk data, StringBuilder stringBuilder)
    {
        while (dataOffset < data.Data.Length)
        {
            var charBuf = data.Data.AsSpan((int)dataOffset++, 1);
            var modBuf = data.Data.AsSpan((int)dataOffset++, 1);

            if (charBuf[0] == 0 && modBuf[0] == 0)
            {
                break;
            }

            if (modBuf[0] != 0)
            {
                stringBuilder.Append(Encoding.UTF8.GetString(modBuf));
            }

            stringBuilder.Append(Encoding.UTF8.GetString(charBuf));
        }
    }
}