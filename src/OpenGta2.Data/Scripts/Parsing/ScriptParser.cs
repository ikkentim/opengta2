using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OpenGta2.Data.Scripts;

public class ScriptParser
{
    public Script Parse(Stream stream)
    {
        // GTA2 script file format:
        // - POINTERS: 6000 word values which point to functions within the script
        // - SCRIPT: 65536 bytes of script binary data
        // - STRING TABLE:
        //   - LENGTH: a word which indicates the length of the string table data
        //   - STRINGS:
        //     - TYPE: dword the type of the string
        //     - LENGTH: a byte which contains the length of the string
        //     - VALUE: a null terminated string value

        var pointers = ReadPointers(stream);
        var scriptData = ReadScript(stream);

        var strings = ReadStrings(stream);

        return new Script(pointers, scriptData, strings);
    }

    private static ushort[] ReadPointers(Stream stream)
    {
        var pointers = new ushort[6000];
        var bPointers = MemoryMarshal.Cast<ushort, byte>(pointers.AsSpan());

        Debug.Assert(bPointers.Length == 12000);

        var length = stream.Read(bPointers);

        if (length != bPointers.Length)
        {
            throw new Exception("bad read");
        }

        return pointers;
    }

    private static byte[] ReadScript(Stream stream)
    {
        var scriptData = new byte[65536];
        var length = stream.Read(scriptData.AsSpan());

        if (length != scriptData.Length)
        {
            throw new Exception("bad read");
        }

        return scriptData;
    }

    private static StringTable ReadStrings(Stream stream)
    {
        var tableLength = stream.ReadExactWord();
        var read = 0;
        var strings = new Dictionary<uint, StringValue>();

        while (read < tableLength)
        {
            var id = stream.ReadExactDoubleWord();
            var type = (StringType)stream.ReadExactDoubleWord();
            var length = stream.ReadExactByte();
            var value = stream.ReadExactString(length);

            read += 9 + length;

            strings[id] = new StringValue(type, length, value);
        }

        return new StringTable(strings);
    }
}